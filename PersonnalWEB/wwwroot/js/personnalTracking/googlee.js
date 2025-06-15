
function selectDepartment(department) {
    dep = department;
}

function formatDate(date) {
    // Yıl, ay ve günü al
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Ayı alırken 0 tabanlı olduğu için +1 ekliyoruz
    const day = String(date.getDate()).padStart(2, '0');

    // Formatı oluştur ve geri döndür
    return `${month}-${day}-${year}`;
}
document.addEventListener("DOMContentLoaded", function () {
    document.getElementById('submitButton-Vpn').addEventListener('click', function () {
        fetchDataAndDraw(); // Butona tıklandığında bu fonksiyonu çağır
    });
});

function drawEmptyChart() {
    const data = {
        labels: ['Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma'],
        datasets: [{
            label: 'Sayımlar',
            backgroundColor: 'rgba(255, 99, 132, 0.5)',
            data: [5, 2, 4, 9, 3]
        }]
    };



    const options = {
        title: {
            display: true,
            text: 'Haftaiçi Günlerde Kategorilere Göre Çalışan Sayıları'
        },
        scales: {
            xAxes: [{
                scaleLabel: {
                    display: true,
                    labelString: 'Günler'
                }
            }],
            yAxes: [{
                scaleLabel: {
                    display: true,
                    labelString: 'Sayım'
                },
                ticks: {
                    beginAtZero: true
                }
            }]
        }
    };

    const ctx = document.getElementById('chart_div').getContext('2d');
    const chart = new Chart(ctx, {
        type: 'bar',
        data: data,
        options: options
    });
}

function fetchDataAndDraw() {
    const startDate = new Date(document.getElementById('date-input-vpn').value);
    const endDate = new Date(document.getElementById('date-input-vpn2').value);
    const formattedStartDate = formatDate(startDate);
    const formattedEndDate = formatDate(endDate);
    const year = startDate.getFullYear();
    //const selectedWeekText = document.getElementById('weekDropdown').textContent;
    //const week = parseInt(selectedWeekText.replace('Hafta ', ''), 10);
    var dep = "";

    // console.log(dep);


    //selectDepartment(dep);

    $.ajax({
        url: '/PersonnalTracking/GetLatesWithDepartment',
        method: 'get',
        data: {
            startDate: formattedStartDate,
            endDate: formattedEndDate,
            year: year,
            Department: selectedDepartment
        },
        success: function (response) {
            const processedData = processData(response.data);
            drawChart(processedData);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.status == 400) {
                alert('Bir hata oluştu: ' + jqXHR.responseText);
            }
        }
    });
}

function processData(apiData) {
    const daysOfWeek = ["Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma"];
    const tempValues = new Set();
    const counts = {};

    const output = [['Gün']];

    for (let i = 0; i < daysOfWeek.length; i++) {
        output.push([daysOfWeek[i]]);
    }

    for (const entry of apiData) {
        tempValues.add(entry.processTemp);
    }
    const tempArray = [...tempValues].sort((a, b) => a - b);

    for (const value of tempArray) {
        output[0].push(value.toString());
    }

    for (const day of daysOfWeek) {
        counts[day] = {};
        for (const value of tempArray) {
            counts[day][value] = 0;
        }
    }

    for (const entry of apiData) {
        for (const employee of entry.employees) {
            const day = getDayFromFirstRecord(employee.firstRecord);
            if (daysOfWeek.includes(day)) {
                counts[day][entry.processTemp]++;
            }
        }
    }

    for (let i = 1; i <= daysOfWeek.length; i++) {
        for (const value of tempArray) {
            output[i].push(counts[daysOfWeek[i - 1]][value]);
        }
    }

    let customTicks = [];
    for (const entry of apiData) {
        customTicks.push({
            v: entry.processTemp,
            f: entry.message
        });
    }

    return {
        chartData: output,
        ticks: customTicks
    };
}

function drawChart(dataObject) {
    const data = {
        labels: dataObject.chartData[0].slice(1),
        datasets: []
    };

    for (let i = 1; i < dataObject.chartData.length; i++) {
        data.datasets.push({
            label: dataObject.chartData[i][0],
            backgroundColor: 'rgba(54, 162, 235, 0.5)',
            data: dataObject.chartData[i].slice(1)
        });
    }

    const options = {
        title: {
            display: true,
            text: 'Hafta İçi Günlerde Kategorilere Göre Sayımlar'
        },
        scales: {
            xAxes: [{
                scaleLabel: {
                    display: true,
                    labelString: 'Günler'
                }
            }],
            yAxes: [{
                scaleLabel: {
                    display: true,
                    labelString: 'Sayım'
                },
                ticks: {
                    beginAtZero: true
                }
            }]
        },
        legend: {
            position: 'top',
            labels: {
                fontSize: 12
            }
        },
        animation: {
            duration: 1000,
            easing: 'out'
        },
        height: 600
    };

    const ctx = document.getElementById('chart_div').getContext('2d');
    const chart = new Chart(ctx, {
        type: 'bar',
        data: data,
        options: options
    });

    updateRatioInfo(originalData);
}

function updateRatioInfo(originalData) {
    if (!originalData || !originalData.length) return;

    let totalEmployees = 0;
    const counts = {};

    originalData.forEach(item => {
        totalEmployees += item.employees.length;
        counts[item.processTemp] = counts[item.processTemp] || { count: 0, message: item.message };
        counts[item.processTemp].count += item.employees.length;
    });

    const chartData = [['ProcessTemp', 'Percentage']];

    Object.keys(counts).forEach(processTemp => {
        const ratio = (counts[processTemp].count / totalEmployees) * 100;
        chartData.push([counts[processTemp].message, ratio]);
    });

    drawPieChart(chartData);
}

function drawPieChart(chartData) {
    google.charts.load('current', { 'packages': ['corechart'] });
    google.charts.setOnLoadCallback(drawChartPie);

    function drawChartPie() {
        const data = google.visualization.arrayToDataTable(chartData);
        const options = {
            title: 'Oranlar',
            titleTextStyle: {
                fontSize: 24,
                bold: true
            },
            subtitle: 'Çalışanların ProcessTemp Dağılımı',
            subtitleTextStyle: {
                fontSize: 14
            },
            animation: {
                duration: 1000,
                startup: true
            },
            height: 600,
            colors: ['#3366CC', '#DC3912', '#FF9900', '#109618', '#990099'],
            legend: {
                position: 'right',
                alignment: 'center',
                textStyle: {
                    fontSize: 12
                }
            },
            is3D: true
        };
        const chart = new google.visualization.PieChart(document.getElementById('piechart'));
        chart.draw(data, options);
    }
}

function formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${month}-${day}-${year}`;
}

function getDayFromFirstRecord(dateTimeStr) {
    const parts = dateTimeStr.split('T');
    const dateParts = parts[0].split('-');
    const year = parseInt(dateParts[0], 10);
    const month = parseInt(dateParts[1], 10) - 1;
    const day = parseInt(dateParts[2], 10);
    const date = new Date(year, month, day);
    const days = ["Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi"];
    return days[date.getDay()];
}
