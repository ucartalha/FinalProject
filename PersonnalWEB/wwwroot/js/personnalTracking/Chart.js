
function fetchPerformanceData(startDate, endDate, departmentId) {
    const url = `/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`;

    fetch(url)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response; // camelCase dönüşümü
            const chartData = processChartData(data);
            drawChart(chartData);
            fillTable(chartData); // Eğer yoksa, bu fonksiyonu eklemelisin ya da çağrıyı kaldırmalısın
        })
        .catch(err => console.error("Veri alınamadı:", err));
}

function processChartData(data) {
    const result = [];
    const grouped = {};

    data.forEach(item => {
        const key = `${item.name} ${item.surName}`;
        if (!grouped[key]) {
            grouped[key] = {
                fullName: key,
                officeHours: 0,
                vpnHours: 0
            };
        }

        if (item.workingHour) {
            grouped[key].officeHours += timeStringToHours(item.workingHour);
        }

        if (item.duration) {
            grouped[key].vpnHours += timeStringToHours(item.duration);
        }
    });

    for (const emp in grouped) {
        result.push(grouped[emp]);
    }

    return result;
}

function timeStringToHours(str) {
    const [h, m, s] = str.split(':').map(Number);
    return h + (m / 60) + (s / 3600);
}

// Grafik cache kontrolü
let departmentChartInstance = null;

function drawChart(chartData) {
    const canvas = document.getElementById("departmentChart");
    if (!canvas) {
        console.error("departmentChart canvas elementi bulunamadı.");
        return;
    }

    const ctx = canvas.getContext("2d");

    const labels = chartData.map(x => x.fullName);
    const office = chartData.map(x => Number(x.officeHours.toFixed(2)));
    const vpn = chartData.map(x => Number(x.vpnHours.toFixed(2)));

    // Önceki chart varsa yok et
    if (departmentChartInstance) {
        departmentChartInstance.destroy();
    }

    departmentChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Ofis Saat',
                    data: office,
                    backgroundColor: '#4CAF50'
                },
                {
                    label: 'VPN Saat',
                    data: vpn,
                    backgroundColor: '#2196F3'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top'
                },
                title: {
                    display: true,
                    text: 'Çalışanların Ofis & VPN Süreleri'
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}
