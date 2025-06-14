document.addEventListener("DOMContentLoaded", function () {
    loadDepartments();
    setDefaultDates();

    document.getElementById("btnFilter")?.addEventListener("click", function () {
        const startDate = document.getElementById("startDate").value;
        const endDate = document.getElementById("endDate").value;
        const departmentId = document.getElementById("departmentSelect").value;
        fetchAndRenderPerformance(startDate, endDate, departmentId);
        fetchAndRenderStats(startDate, endDate, departmentId);
        fetchAndRenderTopEmployees(startDate, departmentId);
    });
});

function setDefaultDates() {
    const now = new Date();
    const past = new Date();
    past.setDate(now.getDate() - 30);
    document.getElementById("startDate").value = past.toISOString().split('T')[0];
    document.getElementById("endDate").value = now.toISOString().split('T')[0];
}

function loadDepartments() {
    fetch("/PersonnalTracking/GetDepartments")
        .then(res => res.json())
        .then(data => {
            const select = document.getElementById("departmentSelect");
            data.data.forEach(dept => {
                const option = document.createElement("option");
                option.value = dept.id;
                option.textContent = dept.name;
                select.appendChild(option);
            });
        });
}

function fetchAndRenderPerformance(startDate, endDate, departmentId) {
    const url = `/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`;
    fetch(url)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response;
            const chartData = processChartData(data);
            drawChart(chartData);
            fillTable(chartData);
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
                vpnHours: 0,
                lateDays: 0
            };
        }

        if (item.workingHour) {
            grouped[key].officeHours += timeStringToHours(item.workingHour);
        }

        if (item.duration) {
            grouped[key].vpnHours += timeStringToHours(item.duration);
        }

        const lateOffice = new Date(item.firstRecord).getHours() + (new Date(item.firstRecord).getMinutes() / 60);
        const lateVpn = new Date(item.vpnFirstRecord).getHours() + (new Date(item.vpnFirstRecord).getMinutes() / 60);
        if (lateOffice > 8.5 || lateVpn > 8.5) grouped[key].lateDays++;
    });

    for (const emp in grouped) {
        result.push(grouped[emp]);
    }

    return result;
}

function timeStringToHours(str) {
    if (!str || !str.includes(":")) return 0;
    const [h, m, s] = str.split(':').map(Number);
    return h + (m / 60) + (s / 3600);
}

function drawChart(chartData) {
    const ctx = document.getElementById("departmentChart").getContext("2d");
    const labels = chartData.map(x => x.fullName);
    const office = chartData.map(x => x.officeHours.toFixed(2));
    const vpn = chartData.map(x => x.vpnHours.toFixed(2));

    if (window.performanceChart) {
        window.performanceChart.destroy();
    }

    window.performanceChart = new Chart(ctx, {
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
            plugins: {
                legend: {
                    position: 'top'
                },
                title: {
                    display: true,
                    text: 'Çalışanların Ofis & VPN Süreleri'
                }
            }
        }
    });
}

function fillTable(chartData) {
    const tbody = document.querySelector("#performanceTable tbody");
    tbody.innerHTML = "";

    chartData.forEach(item => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${item.fullName}</td>
            <td>${(item.officeHours + item.vpnHours).toFixed(2)}</td>
            <td>${item.officeHours.toFixed(2)}</td>
            <td>${item.vpnHours.toFixed(2)}</td>
            <td>${item.lateDays}</td>
        `;
        tbody.appendChild(row);
    });
}

function fetchAndRenderStats(startDate, endDate, departmentId) {
    const url = `/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`;
    fetch(url)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response;
            const uniqueEmployees = new Set();
            let totalHours = 0;
            let totalVisits = 0;

            data.forEach(e => {
                const h = timeStringToHours(e.toplamZaman || e.workingHour || "0:00:00");
                totalHours += h;
                if (h > 1) totalVisits++;
                uniqueEmployees.add(e.remoteEmployeeId);
            });

            const avg = totalHours / (totalVisits || 1);

            document.getElementById('GetCustomerAmount').innerText = uniqueEmployees.size;
            document.getElementById('GetCustomerAmountByStatutrue').innerText = totalHours.toFixed(2);
            document.getElementById('GetDiscountAmount').innerText = avg.toFixed(2);
        });
}

function fetchAndRenderTopEmployees(startDate, departmentId) {
    const date = new Date(startDate);
    const month = date.getMonth() + 1;
    const year = date.getFullYear();

    fetch(`/PersonnalTracking/BestPersonalMonth?month=${month}&year=${year}&departmentId=${departmentId}`)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response;
            const tbody = document.querySelector("#employeeTable tbody");
            tbody.innerHTML = "";

            data.data.forEach(item => {
                const total = convertDay(item.averageHour);
                const weekday = parseDuration(item.totalHour);
                const weekend = parseDuration(item.weekendTotalHour);

                const row = document.createElement("tr");
                row.innerHTML = `
                    <td>${item.name}</td>
                    <td>${total.toFixed(2)}</td>
                    <td>${weekday.toFixed(2)}</td>
                    <td>${weekend.toFixed(2)}</td>
                `;
                tbody.appendChild(row);
            });
        });
}

function parseDuration(str) {
    const [h, m, s] = str.split(":").map(Number);
    return h + m / 60 + s / 3600;
}

function convertDay(str) {
    if (!str || !str.includes('.')) return parseDuration(str);
    const [d, t] = str.split('.');
    const [h, m, s] = t.split(":").map(Number);
    return (+d * 24) + h + m / 60 + s / 3600;
}
