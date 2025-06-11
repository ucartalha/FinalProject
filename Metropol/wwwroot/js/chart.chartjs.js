$(function(){
  $('#submitButton').click(function() {
    var name = $('#nameInput').val();
    var date = $('#dateInput').val();

    // API'den verileri al
    $.ajax({
      url: 'http://localhost:47153/Personal/process-monthly-average',
      method: 'GET',
      data: {
        name: name,
        month: date
      },
      dataType: 'json',
      success: function(response) {
        var data = response.data; // API'den gelen veri

        // Etiketler ve veriler için boş diziler oluştur
        var labels = []; // Etiketler için boş bir dizi oluştur
        var votes = []; // Oy sayıları için boş bir dizi oluştur

        // Verileri döngü ile işle
        data.forEach(function(item) {
          var date2 = new Date(item.date);
          var day = date2.getDate();
          var month = date2.getMonth() + 1;
          var year = date2.getFullYear();

          var formattedDate = year + "-" + ("0" + month).slice(-2) + "-" + ("0" + day).slice(-2);
          labels.push(formattedDate);

          var date1 = new Date('1970-01-01T' + item.averageHour);
          var hours = date1.getHours();
          var minutes = date1.getMinutes();

          var totalMinutes = hours * 60 + minutes; // Saati dakikaya çevir
          votes.push(totalMinutes); // Dakika değerini ekle
        });
        console.log(votes);
        console.log(labels);
        console.log(response.data);
        // Grafik verileri
        const chartData = {
          labels: labels,
          datasets: [{
            label: 'Ortalama Çalışma Saati',
            backgroundColor: 'rgba(255, 99, 132, 0.5)',
            borderColor: 'rgba(255, 99, 132, 1)',
            fill: false,
            data: votes.map(Number),
          }]
        };
        chartData.datasets[0].data = chartData.datasets[0].data.map(function(minutes) {
          var hours = Math.floor(minutes / 60); // Saat
          var minutesRemainder = minutes % 60; // Dakika
          var totalHours=hours + minutesRemainder / 60;
          return totalHours.toFixed(1);  // Saat olarak güncelle
        });
          if (data.length === 0) {
              // Eğer API'den veri gelmezse, varsayılan verileri kullan
              labels = ['Jan', 'Feb', 'Mar'];
              votes = [12, 39, 20,];
          }

        // Grafik ayarları
        const config = {
          type: 'bar',
          data: chartData,
          options: {
            responsive: true,
            scales: {
              x: {
                display: true,
                title: {
                  display: true,
                  text: 'Month'
                }
              },
              y: {
                display: true,
                title: {
                  display: true,
                  text: 'Value'
                }
              }
            },
            plugins: {
              tooltip: {
                enabled: false,
                external: function(context) {
                  var tooltip = document.getElementById('chart-tooltip');
                  if (!tooltip) {
                    tooltip = document.createElement('div');
                    tooltip.id = 'chart-tooltip';
                    tooltip.innerHTML = '';
                    tooltip.style.backgroundColor = 'rgba(0,0,0,0.7)';
                    tooltip.style.borderRadius = '3px';
                    tooltip.style.color = '#fff';
                    tooltip.style.padding = '5px';
                    tooltip.style.position = 'absolute';
                    tooltip.style.zIndex = '9999';
                    document.body.appendChild(tooltip);
                  }
                   
                  var event = context.tooltipEvent;
                  var activeElements = context.chart.getElementsAtEventForMode(event, 'nearest', { intersect: true }, false);

                  if (activeElements && activeElements.length > 0) {
                    var element = activeElements[0];
                    var datasetIndex = element.datasetIndex;
                    var index = element.index;
                    var dataPoint = context.chart.config.data.datasets[datasetIndex].data[index];

                    if (dataPoint) {
                      var averageHour = dataPoint.averageHour; // averageHour değerine erişim
              
                      // averageHour değerini saat ve dakika olarak göstermek için
                      var date = new Date(averageHour);
                      var hours = date.getHours();
                      var minutes = date.getMinutes();
                      var time = ("0" + hours).slice(-2) + ":" + ("0" + minutes).slice(-2);
              
                      tooltip.style.display = 'block';
                      tooltip.style.left = event.clientX + 'px';
                      tooltip.style.top = event.clientY + 'px';
                      tooltip.innerHTML = date + ' ' + time;
                    }  else {
                      tooltip.style.display = 'none';
                    }
                  } else {
                    tooltip.style.display = 'none';
                  }
                }
              }
            }
          }
        };

        var ctx1 = document.getElementById('chartBar1').getContext('2d');
        // Grafik oluştur ve veriyi ata
        var chart = new Chart(ctx1, config);
      },
      error: function(error) {
        console.log(error);
      }
    });
  });

  
  $('#submitButton').click(function() {
    var name = $('#nameInput').val();
    var date = $('#dateInput').val();
  
    // API'den verileri al
    $.ajax({
      url: 'http://localhost:47153/Personal/process-monthly-average',
      method: 'GET',
      data: {
        name: name,
        month: date
      },
      dataType: 'json',
      success: function(response) {
        var data = response.data; // API'den gelen veri
  
        // Etiketler ve veriler için boş diziler oluştur
        var labels = []; // Etiketler için boş bir dizi oluştur
        var votes = []; // Oy sayıları için boş bir dizi oluştur
  
        // Verilere göre döngü ile işlem yap
        data.forEach(function(item) {
          // API'den gelen veri yapısına göre verileri al
          var dateValue = item.date; // Tarih değeri
          var remoteHour = item.remoteHour; // Uzaktan çalışma saati
  
          // Etiketleri ve verileri doldur
          labels.push(dateValue);
  
          // API'den gelen veri yapısına göre uzaktan çalışma saati değerini işle
          // Eğer uzaktan çalışma saati string bir formatta ise uygun dönüşümü yapmalısınız
          var totalMinutes = 0;
if (typeof remoteHour === 'string') {
  var parts = remoteHour.split(':');
  var hours = parseInt(parts[0]);
  var minutes = parseInt(parts[1]);
  totalMinutes = hours * 60 + minutes;
} else if (typeof remoteHour === 'number') {
  totalMinutes = Math.floor(remoteHour / 60); // Saniyeden dakikaya çevir
}
votes.push(totalMinutes);
        });
  
        // Grafik verileri
        const chartData = {
          labels: labels,
          datasets: [{
            label: 'Ortalama Uzaktan Çalışma Saati',
            backgroundColor: 'rgba(255, 99, 132, 0.5)',
            borderColor: 'rgba(255, 99, 132, 1)',
            fill: false,
            data: votes.map(Number),
          }]
        };
        chartData.datasets[0].data = chartData.datasets[0].data.map(function(minutes) {
          // var hours = Math.floor(minutes / 60); // Saat
          // var minutesRemainder = minutes % 60; // Dakika
  
          // return hours + minutesRemainder / 60; // Saat olarak güncelle
          var hours = minutes / 60; // Saat
  
          return hours.toFixed(1);
        });
  
        // Grafik ayarları
        const config = {
          type: 'bar',
          data: chartData,
          options: {
            responsive: true,
            scales: {
              x: {
                display: true,
                title: {
                  display: true,
                  text: 'Month'
                }
              },
              y: {
                display: true,
                title: {
                  display: true,
                  text: 'Value'
                }
              }
            },
            plugins: {
              tooltip: {
                enabled: false,
                external: function(context) {
                  var tooltip = document.getElementById('chart-tooltip');
                  if (!tooltip) {
                    tooltip = document.createElement('div');
                    tooltip.id = 'chart-tooltip';
                    tooltip.innerHTML = '';
                    tooltip.style.backgroundColor = 'rgba(0,0,0,0.7)';
                    tooltip.style.borderRadius = '3px';
                    tooltip.style.color = '#fff';
                    tooltip.style.padding = '5px';
                    tooltip.style.position = 'absolute';
                    tooltip.style.zIndex = '9999';
                    document.body.appendChild(tooltip);
                  }
  
                  var event = context.tooltipEvent;
                  var activeElements = context.chart.getElementsAtEventForMode(event, 'nearest', { intersect: true }, false);
  
                  if (activeElements && activeElements.length > 0) {
                    var element = activeElements[0];
                    var datasetIndex = element.datasetIndex;
                    var index = element.index;
                    var dataPoint = context.chart.config.data.datasets[datasetIndex].data[index];
  
                    if (dataPoint) {
                      var remoteHour = dataPoint.remoteHour; // remoteHour değerine erişim
  
                      // remoteHour değerini saat ve dakika olarak göstermek için
                      var date = new Date(remoteHour);
                      var hours = date.getHours();
                      var minutes = date.getMinutes();
                      var time = ("0" + hours).slice(-2) + ":" + ("0" + minutes).slice(-2);
                      
                      
  
                      tooltip.style.display = 'block';
                      tooltip.style.left = event.clientX + 'px';
                      tooltip.style.top = event.clientY + 'px';
                      tooltip.innerHTML = date + ' ' + time;
                    } else {
                      tooltip.style.display = 'none';
                    }
                  } else {
                    tooltip.style.display = 'none';
                  }
                }
              }
            }
          }
        };
  
        var ctx2 = document.getElementById('chartBar2').getContext('2d');
// Grafik oluştur ve veriyi ata
var chart2 = new Chart(ctx2, config);
      },
      error: function(error) {
        console.log(error);
      }
    });
  });

  var ctx3 = document.getElementById('chartBar3').getContext('2d');

  var gradient = ctx3.createLinearGradient(0, 0, 0, 250);
  gradient.addColorStop(0, '#560bd0');
  gradient.addColorStop(1, '#00cccc');

  new Chart(ctx3, {
    type: 'bar',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        label: '# of Votes',
        data: [12, 39, 20, 10, 25, 18],
        backgroundColor: gradient
      }]
    },
    options: {
      maintainAspectRatio: false,
      responsive: true,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 10,
            max: 80
          }
        }],
        xAxes: [{
          barPercentage: 0.6,
          ticks: {
            beginAtZero:true,
            fontSize: 11
          }
        }]
      }
    }
  });

  var ctx4 = document.getElementById('chartBar4').getContext('2d');
  new Chart(ctx4, {
    type: 'horizontalBar',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        label: '# of Votes',
        data: [12, 39, 20, 10, 25, 18],
        backgroundColor: ['#560bd0', '#007bff','#00cccc','#cbe0e3','#74de00','#f10075']
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 10,
          }
        }],
        xAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 11,
            max: 80
          }
        }]
      }
    }
  });

  var ctx5 = document.getElementById('chartBar5').getContext('2d');
  new Chart(ctx5, {
    type: 'horizontalBar',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        data: [12, 39, 20, 10, 25, 18],
        backgroundColor: ['#560bd0', '#007bff','#74de00','#f10075','#74de00','#f10075']
      }, {
        data: [22, 30, 25, 30, 20, 40],
        backgroundColor: '#cad0e8'
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 11,
          }
        }],
        xAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 11,
            max: 80
          }
        }]
      }
    }
  });

  /** STACKED BAR CHART **/
  var ctx6 = document.getElementById('chartStacked1');
  new Chart(ctx6, {
    type: 'bar',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        data: [10, 24, 20, 25, 35, 50],
        backgroundColor: '#314d83',
        borderWidth: 1,
        fill: true
      },{
        data: [10, 24, 20, 25, 35, 50],
        backgroundColor: '#007bff',
        borderWidth: 1,
        fill: true
      },{
        data: [20, 30, 28, 33, 45, 65],
        backgroundColor: '#cad0e8',
        borderWidth: 1,
        fill: true
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          stacked: true,
          ticks: {
            beginAtZero:true,
            fontSize: 11
          }
        }],
        xAxes: [{
          barPercentage: 0.5,
          stacked: true,
          ticks: {
            fontSize: 11
          }
        }]
      }
    }
  });

  var ctx7 = document.getElementById('chartStacked2');
  new Chart(ctx7, {
    type: 'horizontalBar',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        data: [10, 24, 20, 25, 35, 50],
        backgroundColor: '#314d83',
        borderWidth: 1,
        fill: true
      },{
        data: [10, 24, 20, 25, 35, 50],
        backgroundColor: '#007bff',
        borderWidth: 1,
        fill: true
      },{
        data: [20, 30, 28, 33, 45, 65],
        backgroundColor: '#cad0e8',
        borderWidth: 1,
        fill: true
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          stacked: true,
          ticks: {
            beginAtZero:true,
            fontSize: 10,
            max: 80
          }
        }],
        xAxes: [{
          stacked: true,
          ticks: {
            beginAtZero:true,
            fontSize: 11
          }
        }]
      }
    }
  });

  /* LINE CHART */
  var ctx8 = document.getElementById('chartLine1');
  new Chart(ctx8, {
    type: 'line',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'July', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      datasets: [{
        data: [12, 15, 18, 40, 35, 38, 32, 20, 25, 15, 25, 30],
        borderColor: '#f10075',
        borderWidth: 1,
        fill: false
      },{
        data: [10, 20, 25, 55, 50, 45, 35, 30, 45, 35, 55, 40],
        borderColor: '#007bff',
        borderWidth: 1,
        fill: false
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 10,
            max: 80
          }
        }],
        xAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 11
          }
        }]
      }
    }
  });

  /** AREA CHART **/
  var ctx9 = document.getElementById('chartArea1');

  var gradient1 = ctx3.createLinearGradient(0, 350, 0, 0);
  gradient1.addColorStop(0, 'rgba(241,0,117,0)');
  gradient1.addColorStop(1, 'rgba(241,0,117,.5)');

  var gradient2 = ctx3.createLinearGradient(0, 280, 0, 0);
  gradient2.addColorStop(0, 'rgba(0,123,255,0)');
  gradient2.addColorStop(1, 'rgba(0,123,255,.3)');

  new Chart(ctx9, {
    type: 'line',
    data: {
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'July', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
      datasets: [{
        data: [12, 15, 18, 40, 35, 38, 32, 20, 25, 15, 25, 30],
        borderColor: '#f10075',
        borderWidth: 1,
        backgroundColor: gradient1
      },{
        data: [10, 20, 25, 55, 50, 45, 35, 37, 45, 35, 55, 40],
        borderColor: '#007bff',
        borderWidth: 1,
        backgroundColor: gradient2
      }]
    },
    options: {
      maintainAspectRatio: false,
      legend: {
        display: false,
          labels: {
            display: false
          }
      },
      scales: {
        yAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 10,
            max: 80
          }
        }],
        xAxes: [{
          ticks: {
            beginAtZero:true,
            fontSize: 11
          }
        }]
      }
    }
  });

  /** PIE CHART **/
  var datapie = {
    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May'],
    datasets: [{
      data: [20,20,30,5,25],
      backgroundColor: ['#560bd0', '#007bff','#00cccc','#cbe0e3','#74de00']
    }]
  };

  var optionpie = {
    maintainAspectRatio: false,
    responsive: true,
    legend: {
      display: false,
    },
    animation: {
      animateScale: true,
      animateRotate: true
    }
  };

  // For a doughnut chart
  var ctx6 = document.getElementById('chartPie');
  var myPieChart6 = new Chart(ctx6, {
    type: 'doughnut',
    data: datapie,
    options: optionpie
  });

  // For a pie chart
  var ctx7 = document.getElementById('chartDonut');
  var myPieChart7 = new Chart(ctx7, {
    type: 'pie',
    data: datapie,
    options: optionpie
  });


});
