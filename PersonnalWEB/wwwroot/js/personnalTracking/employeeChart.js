
$(function () {
    var chart;
    var chart2;
    var selectedUserId;

    const dateInput = document.getElementById('date-input-chart2');

    createDefaultChart();

    $("#userDropdown3").on("click", function (event) {
        var target = event.target;

        if (target.classList.contains("dropdown-item")) {
            // Tıklanan çalışanı seçili yap
            target.classList.add("active");

            // Seçilen çalışanın ID'sini değişkene ata
            selectedUserId = target.getAttribute("data-id");

            // Verileri çek ve grafikleri oluştur
            fetchDataAndCreateCharts();
        }
    });

    // Submit butonuna tıklama olayını dinle
    $('#submitButton-Vpn').click(function () {
        // Seçilen tarih bilgisini al
        var selectedDate = new Date(document.getElementById('date-input-vpn').value);
        var year = selectedDate.getFullYear();
        var month = selectedDate.getMonth() + 1;

        // Verileri çek ve grafikleri oluştur
        fetchDataAndCreateCharts(year, month);
    });

    // Verileri API'den çek ve grafikleri oluştur
    function fetchDataAndCreateCharts(year, month) {
        if (chart) {
            chart.destroy(); // Eğer birinci grafik varsa, önceki grafikleri yok et
        }
        if (chart2) {
            chart2.destroy(); // Eğer ikinci grafik varsa, önceki grafikleri yok et
        }

        // API'den verileri al
        $.ajax({
            url: '/PersonnalTracking/ProcessMonthlyAverage?Id=' + selectedUserId + '&month=' + month + '&year=' + year,
            method: 'GET',
            data: {
                Id: selectedUserId,
                month: month,
                year: year
            },
            dataType: 'json',
            success: function (response) {
                var data = response.data; // API'den gelen veri

                // İlk grafik verileri
                var labels = [];
                var votes = [];
                var workDays = [];

                // Verileri döngü ile işle
                data.forEach(function (item) {
                    var date2 = new Date(item.date);
                    var day = date2.getDate();
                    var month = date2.getMonth() + 1;
                    var year = date2.getFullYear();



                    var formattedDate = ("0" + month).slice(-2) + "-" + year;
                    labels.push(formattedDate);

                    var date1 = new Date('1970-01-01T' + item.averageHour);
                    var hours = date1.getHours();
                    var minutes = date1.getMinutes();

                    var totalMinutes = hours * 60 + minutes;
                    votes.push(totalMinutes);

                    var workingDays = item.officeDay; // item içerisindeki çalışma gününü varsayıyorum
                    workDays.push(workingDays);
                });

                const chartData = {
                    labels: labels,
                    datasets: [
                        {
                            label: 'Ortalama Çalışma Saati',
                            backgroundColor: '#833ab45c',
                            borderColor: '#833ab45c',
                            fill: false,
                            data: votes.map(Number),
                            yAxisID: 'y-axis-1',
                        },
                        {
                            label: 'Çalışma Gün Sayısı',
                            backgroundColor: '#833ab4',
                            borderColor: '#833ab4',
                            fill: false,
                            data: workDays.map(Number),
                            yAxisID: 'y-axis-2',
                        }
                    ]
                };

                const config = {
                    type: 'bar',
                    data: chartData,
                    options: {
                        scales: {
                            yAxes: [
                                {
                                    id: 'y-axis-1',
                                    type: 'linear',
                                    position: 'left',
                                    ticks: {
                                        stepSize: 60, // 1 saatlik adımlar
                                        callback: function (value) {
                                            var hours = Math.floor(value);

                                            return hours;
                                        }
                                    },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'Ortalama Çalışma Saati'
                                    }
                                },
                                {
                                    id: 'y-axis-2',
                                    type: 'linear',
                                    position: 'right',
                                    ticks: {
                                        beginAtZero: true
                                    },
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

                chartData.datasets[0].data = chartData.datasets[0].data.map(function (minutes) {
                    var hours = Math.floor(minutes / 60);
                    var minutesRemainder = minutes % 60;
                    var totalHours = hours + minutesRemainder / 60;
                    return totalHours.toFixed(1);
                });

                var ctx1 = document.getElementById('chartBar1').getContext('2d');
                chart = new Chart(ctx1, config);

                // İkinci grafik verileri
                var labels2 = labels; // Tarihleri kullan
                var votes2 = [];
                var averageHours = [];
                var workDays2 = [];// Aylık verileri saklamak için bir nesne

                data.forEach(function (item) {
                    var date2 = new Date(item.date);
                    var day = date2.getDate();
                    var month = date2.getMonth() + 1;
                    var year = date2.getFullYear();

                    // Uzaktan çalışma saatini saniyeden saate çevir
                    var remoteHourInSeconds = item.remoteHour;
                    var hours = Math.floor(remoteHourInSeconds / 3600); // Saat
                    var minutes = Math.floor((remoteHourInSeconds % 3600) / 60); // Dakika
                    var seconds = remoteHourInSeconds % 60; // Saniye
                    var formattedRemoteHour = hours + minutes / 60 + seconds / 3600;
                    formattedRemoteHour = formattedRemoteHour.toFixed(1);
                    averageHours.push(formattedRemoteHour); // Ortalama saatleri kullan

                    var VpnDays = item.vpnDay; // item içerisindeki çalışma gününü varsayıyorum
                    workDays2.push(VpnDays);
                });

                // İkinci grafik verilerini hazırlayın
                const chartData2 = {
                    labels: labels2, // Tarihleri kullan
                    datasets: [{
                        label: 'Ortalama Uzaktan Çalışma Saati',
                        backgroundColor: '#f271215c', // Renk değiştirildi
                        borderColor: '#f271215c', // Renk değiştirildi
                        fill: false,
                        data: averageHours.map(Number),
                        yAxisID: 'y-axis-1',// Ortalama saatleri kullan
                    },
                    {
                        label: 'Çalışma Gün Sayısı',
                        backgroundColor: 'rgb(242, 113, 33)',
                        borderColor: 'rgb(242, 113, 33)',
                        fill: false,
                        data: workDays2.map(Number),
                        yAxisID: 'y-axis-2',
                    }
                    ]
                };

                const config2 = {
                    type: 'bar',
                    data: chartData2,
                    options: {
                        scales: {
                            yAxes: [
                                {
                                    id: 'y-axis-1',
                                    type: 'linear',
                                    position: 'left',
                                    ticks: {
                                        stepSize: 60, // 1 saatlik adımlar
                                        callback: function (value) {
                                            var hours = Math.floor(value);

                                            return hours;
                                        }
                                    },
                                    scaleLabel: {
                                        display: true,
                                        labelString: 'Ortalama Çalışma Saati'
                                    }
                                },
                                {
                                    id: 'y-axis-2',
                                    type: 'linear',
                                    position: 'right',
                                    ticks: {
                                        beginAtZero: true
                                    },
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
                        },
                        plugins: {
                            tooltip: {
                                enabled: false
                            }
                        }
                    }
                };

                var ctx2 = document.getElementById('chartBar2').getContext('2d');
                chart2 = new Chart(ctx2, config2);
            },
            error: function (error) {
                console.log(error);
            }
        });
    }

    // Sayfa yüklendiğinde varsayılan verilerle grafikleri oluştur
    //fetchDataAndCreateCharts();

    function createDefaultChart() {
        var emptyChartData1 = {
            labels: [],
            datasets: [{
                label: 'Ortalama Çalışma Saati',
                backgroundColor: 'rgba(255, 99, 132, 0.5)',
                borderColor: 'rgba(255, 99, 132, 1)',
                fill: false,
                data: [],
            }],
        };

        // İkinci grafik için boş veri kümesi
        var emptyChartData2 = {
            labels: [],
            datasets: [{
                label: 'Ortalama Uzaktan Çalışma Saati',
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
                fill: false,
                data: [],
            }],
        };
        var ctx1 = document.getElementById('chartBar1').getContext('2d');
        chart1 = new Chart(ctx1, {
            type: 'bar',
            data: emptyChartData1,
            options: {
                // Grafik ayarları buraya eklenir
            },
        });

        // İkinci grafik oluştur ve boş veri kümesi ile başlat
        var ctx2 = document.getElementById('chartBar2').getContext('2d');
        chart2 = new Chart(ctx2, {
            type: 'bar',
            data: emptyChartData2,
            options: {
                // Grafik ayarları buraya eklenir
            },
        });
    }
});
