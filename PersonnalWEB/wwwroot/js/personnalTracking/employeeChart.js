$(function () {
    let chart = null;
    let chart2 = null;
    let selectedUserId = null;

    createDefaultChart();

    $("#userDropdown3").on("click", function (event) {
        const target = event.target;
        if (target.classList.contains("dropdown-item")) {
            target.classList.add("active");
            selectedUserId = target.getAttribute("data-id");
            fetchDataAndCreateCharts();
        }
    });

    $('#submitButton-Vpn').click(function () {
        const selectedDate = new Date(document.getElementById('date-input-vpn').value);
        const year = selectedDate.getFullYear();
        const month = selectedDate.getMonth() + 1;

        fetchDataAndCreateCharts(year, month);
    });

    function fetchDataAndCreateCharts(year, month) {
        if (!selectedUserId) {
            console.warn("Çalışan seçilmedi.");
            return;
        }

        if (chart) chart.destroy();
        if (chart2) chart2.destroy();

        $.ajax({
            url: '/PersonnalTracking/ProcessMonthlyAverage',
            method: 'GET',
            data: { Id: selectedUserId, month: month, year: year },
            dataType: 'json',
            success: function (response) {
                const data = response.data;

                if (!Array.isArray(data)) {
                    console.warn("Veri formatı beklenenden farklı:", response);
                    return;
                }

                const labels = [];
                const votes = [];
                const workDays = [];

                data.forEach(function (item) {
                    const dateObj = new Date(item.date);
                    const formattedDate = ("0" + (dateObj.getMonth() + 1)).slice(-2) + "-" + dateObj.getFullYear();
                    labels.push(formattedDate);

                    const avg = new Date('1970-01-01T' + item.averageHour);
                    const totalMinutes = avg.getHours() * 60 + avg.getMinutes();
                    votes.push(totalMinutes);

                    workDays.push(item.officeDay || 0);
                });

                const chartData = {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Ortalama Çalışma Saati',
                            backgroundColor: '#833ab45c',
                            borderColor: '#833ab45c',
                            fill: false,
                            data: votes.map(m => (m / 60).toFixed(1)),
                            yAxisID: 'y-axis-1',
                        },
                        {
                            label: 'Çalışma Gün Sayısı',
                            backgroundColor: '#833ab4',
                            borderColor: '#833ab4',
                            fill: false,
                            data: workDays,
                            yAxisID: 'y-axis-2',
                        }
                    ]
                };

                const config = {
                    type: 'bar',
                    data: chartData,
                    options: {
                        responsive: true,
                        scales: {
                            yAxes: [
                                {
                                    id: 'y-axis-1',
                                    position: 'left',
                                    ticks: {
                                        stepSize: 1,
                                        callback: value => value + ' saat'
                                    },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'Ortalama Çalışma Saati'
                                    }
                                },
                                {
                                    id: 'y-axis-2',
                                    position: 'right',
                                    ticks: { beginAtZero: true },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'Çalışma Gün Sayısı'
                                    }
                                }
                            ],
                            xAxes: [{
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Tarih'
                                }
                            }]
                        }
                    }
                };

                const ctx1 = document.getElementById('chartBar1').getContext('2d');
                chart = new Chart(ctx1, config);

                // İkinci Grafik
                const labels2 = labels;
                const remoteHours = [];
                const vpnDays = [];

                data.forEach(function (item) {
                    const seconds = item.remoteHour || 0;
                    const hours = (seconds / 3600).toFixed(1);
                    remoteHours.push(hours);
                    vpnDays.push(item.vpnDay || 0);
                });

                const chartData2 = {
                    labels: labels2,
                    datasets: [
                        {
                            label: 'Ortalama Uzaktan Çalışma Saati',
                            backgroundColor: '#f271215c',
                            borderColor: '#f271215c',
                            fill: false,
                            data: remoteHours,
                            yAxisID: 'y-axis-1',
                        },
                        {
                            label: 'VPN Gün Sayısı',
                            backgroundColor: 'rgb(242, 113, 33)',
                            borderColor: 'rgb(242, 113, 33)',
                            fill: false,
                            data: vpnDays,
                            yAxisID: 'y-axis-2',
                        }
                    ]
                };

                const config2 = {
                    type: 'bar',
                    data: chartData2,
                    options: {
                        responsive: true,
                        scales: {
                            yAxes: [
                                {
                                    id: 'y-axis-1',
                                    position: 'left',
                                    ticks: {
                                        stepSize: 1,
                                        callback: value => value + ' saat'
                                    },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'Uzaktan Çalışma Saati'
                                    }
                                },
                                {
                                    id: 'y-axis-2',
                                    position: 'right',
                                    ticks: { beginAtZero: true },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'VPN Gün Sayısı'
                                    }
                                }
                            ],
                            xAxes: [{
                                scaleLabel: {
                                    display: true,
                                    labelString: 'Tarih'
                                }
                            }]
                        }
                    }
                };

                const ctx2 = document.getElementById('chartBar2').getContext('2d');
                chart2 = new Chart(ctx2, config2);
            },
            error: function (error) {
                console.error("Veri alınamadı:", error);
            }
        });
    }

    function createDefaultChart() {
        const emptyConfig = {
            type: 'bar',
            data: {
                labels: [],
                datasets: [{
                    label: 'Boş Grafik',
                    backgroundColor: 'rgba(200, 200, 200, 0.5)',
                    data: [],
                }]
            },
            options: {
                responsive: true
            }
        };

        chart = new Chart(document.getElementById('chartBar1').getContext('2d'), emptyConfig);
        chart2 = new Chart(document.getElementById('chartBar2').getContext('2d'), emptyConfig);
    }
});
