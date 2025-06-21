google.charts.load('current', { packages: ['corechart'] });

var selectedDepartment = '';
var selectedUserId = null;

$("#userDropdown3").on("click", ".dropdown-item", function (event) {
    var target = $(this); // Reference to the clicked item
    target.addClass("active");

    // Retrieve the data-id value from the clicked item
    selectedUserId = parseInt(target.attr("data-id"));
    selectedEmployeeName = target.text().trim();

    // Log the values to check if they are correct
    console.log("Selected User ID:", selectedUserId);
    console.log("Selected Employee Name:", selectedEmployeeName);
});

function drawChartTitle(selectedUserName) {
    var chartTitle = 'Çalışan: ' + selectedUserName;
    document.getElementById('chartTitle').innerText = chartTitle;
}

document.getElementById('submitButton-Vpn').addEventListener('click', function () {
    var startDate = document.getElementById('date-input-vpn').value;
    var endDate = document.getElementById('date-input-vpn2').value;

    var controllerUrl = '/PersonnalTracking/GetAllEmployeesWithParams?startDate=' + startDate + '&endDate=' + endDate + '&Id=' + selectedUserId;

    fetch(controllerUrl, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json'
        }
    }).then(response => response.json())
        .then(data => {
            var chartDataMap = new Map();

            data.data.forEach(entry => {
                var date;
                if (entry.date) {
                    date = new Date(entry.date).toLocaleDateString();
                } else {
                    date = new Date(entry.vpnFirstRecord).toLocaleDateString();
                }
                if (!chartDataMap.has(date)) {
                    chartDataMap.set(date, {
                        officeHours: [],
                        vpnHours: [],
                        lateDays: 0
                    });
                }
                chartDataMap.get(date).officeHours.push(entry.workingHour ? calculateHours(entry.workingHour) : 0);
                if (entry.duration) {
                    chartDataMap.get(date).vpnHours.push(calculateHours(entry.duration));
                }
                if (isLate(entry.firstRecord, entry.vpnFirstRecord)) {
                    chartDataMap.get(date).lateDays = 1; // Aynı gün içinde birden fazla geç kalma durumu olmaması için
                }
            });

            var chartDataArray = Array.from(chartDataMap.entries()).map(([date, totals]) => ({
                date: date,
                officeHours: totals.officeHours.reduce((acc, cur) => acc + cur, 0),
                vpnHours: totals.vpnHours.reduce((acc, cur) => acc + cur, 0),
                lateDays: totals.lateDays,
                totalHours: totals.officeHours.reduce((acc, cur) => acc + cur, 0) + totals.vpnHours.reduce((acc, cur) => acc + cur, 0)
            }));

            var sortedDataArray = chartDataArray.sort((a, b) => new Date(a.date) - new Date(b.date));

            // Toplam geç kalınan gün sayısını hesaplayın
            var totalLateDays = sortedDataArray.reduce((acc, cur) => acc + cur.lateDays, 0);

            drawDailyEmployeeChart(sortedDataArray, startDate, endDate, totalLateDays);
        })
        .catch(error => {
            console.error('Veri alınamadı:', error);
        });
});

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('tr-TR'); // Türkçe tarih formatı
}

function calculateHours(timeString) {
    if (!timeString) {
        return 0;
    }
    var timeArray = timeString.split(":");
    return parseFloat(timeArray[0]) + parseFloat(timeArray[1]) / 60 + parseFloat(timeArray[2]) / 3600;
}

function isLate(firstRecord, vpnFirstRecord) {
    // Ofis kaydı için geç kalma durumu
    var firstRecordTime = new Date(firstRecord).getHours() + (new Date(firstRecord).getMinutes() / 60);

    var vpnFirstRecordTime = new Date(vpnFirstRecord).getHours() + (new Date(vpnFirstRecord).getMinutes() / 60);
    if (firstRecord != null && vpnFirstRecord == null) {

        if (firstRecordTime !== null && firstRecordTime > 8.5) {
            return true; // Not late if the office record is on time
        }
    }
    // VPN kaydı için geç kalma durumu
    if (vpnFirstRecord != null && firstRecord == null) {
        // Check if the VPN record exists and is on time
        if (vpnFirstRecordTime !== null && vpnFirstRecordTime > 8.5) {
            return true; // Not late if the VPN record is on time
        }

    }

    if (vpnFirstRecord != null && firstRecord != null) {
        if (vpnFirstRecordTime != null && vpnFirstRecordTime > 8.5 && firstRecordTime > 8.5) {
            return true;
        }
        else {
            return false;
        }
    }
    return false;
}
function parseDate(dateString) {
    var parts = dateString.split('.');
    // Gün, Ay, Yıl sırasına göre tarih parçalarını ayrıştır
    if (parts.length === 3) {
        var day = parseInt(parts[0], 10);
        var month = parseInt(parts[1], 10) - 1; // Ay 0-11 aralığında olmalı
        var year = parseInt(parts[2], 10);
        return new Date(year, month, day);
    }
    return null; // Geçersiz format
}

function drawDailyEmployeeChart(chartDataArray, startDate, endDate, totalLateDays) {
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Tarih');
    data.addColumn('number', 'Ofis Süresi (saat)');
    data.addColumn('number', 'VPN Süresi (saat)');
    data.addColumn('number', 'Geç Kaldı (gün)');
    data.addColumn({ type: 'string', role: 'style' }); // For late bar color
    data.addColumn('number', 'Toplam Çalışma Süresi');
    data.addColumn({ type: 'string', role: 'style' }); // For total hours style

    chartDataArray.forEach(entry => {
        var dateParts = entry.date.split(".");
        var day = parseInt(dateParts[0], 10);
        var month = parseInt(dateParts[1], 10) - 1;
        var year = parseInt(dateParts[2], 10);
        var date = new Date(year, month, day);

        // Gün numarasını al
        var dayOfWeek = date.getDay();
        // Haftasonu kontrolü
        var isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
        var dateString = date.toLocaleDateString('tr-TR');

        if (isWeekend) {
            dateString += ' (H.S)';
        }


        var lateBarColor = entry.lateDays > 0 ? (isWeekend ? 'blue' : 'red') : 'transparent';
        var totalHoursStyle = entry.totalHours > 9.5 ? 'point { size: 10; shape-type: circle; fill-color: green; }' : null;

        data.addRow([dateString, entry.officeHours, entry.vpnHours, entry.lateDays, lateBarColor, entry.totalHours, totalHoursStyle]);
    });

    var options = {
        html: true,
        title: selectedEmployeeName + " " + '`in Günlük Ortalama Ofis ve VPN Süreleri ile Geç Kaldığı Gün Sayısı Tarih Aralığı: ' + startDate + ' - ' + endDate + ' (Toplam Geç Kalınan Gün: ' + totalLateDays + ')',
        isStacked: true,
        hAxis: {
            title: 'Tarih',
            slantedText: true,
            slantedTextAngle: 45,
            textStyle: {
                fontSize: 12 // Yazı boyutunu küçült
            }
        },
        vAxis: {
            title: 'Toplam Süreler (saat)'
        },
        seriesType: 'bars',
        colors: ['#833ab4', '#F27121'],
        series: {
            2: {
                type: 'scatter',
                pointShape: 'circle',
                pointSize: 10,
                color: 'transparent'
            },
            3: {
                type: 'scatter',
                pointShape: 'circle',
                pointSize: 10,
                color: 'transparent'
            }
        },
        legend: { position: 'top' }
    };

    var chart = new google.visualization.ComboChart(document.getElementById('dailyemployee_chart_div'));

    google.visualization.events.addListener(chart, 'select', function () {
        var selection = chart.getSelection();
        if (selection.length > 0) {
            var selectedRow = selection[0].row;

            var selectedDate = data.getValue(selectedRow, 0);
            var selectedOfficeHours = data.getValue(selectedRow, 1);
            var selectedVpnHours = data.getValue(selectedRow, 2);
            var selectedLateDays = data.getValue(selectedRow, 3);

            // Show modal with details
            showDetailsModal(selectedDate, selectedOfficeHours, selectedVpnHours, selectedLateDays, startDate, endDate);
        }
    });

    chart.draw(data, options);
}




function showDetailsModal(selectedDate, officeHours, vpnHours, lateDays, startDate, endDate) {
    // 1) Süreleri ve URL’i hazırla
    const formattedOfficeHours = officeHours.toFixed(2);
    const formattedVpnHours = vpnHours ? vpnHours.toFixed(2) : '0.00';
    const controllerUrl = `/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&Id=${selectedUserId}`;

    // 2) Sunucudan veriyi çek
    fetch(controllerUrl)
        .then(res => res.json())
        .then(({ data }) => {
            // 3) İlgili kaydı bul
            const cleanSelectedDate = selectedDate.replace(' (H.S)', '').trim();
            const details = data.find(entry => {
                const entryDate = entry.date
                    ? new Date(entry.date).toLocaleDateString('tr-TR')
                    : new Date(entry.vpnFirstRecord).toLocaleDateString('tr-TR');

                const vpnDateMatch = entry.vpnFirstRecord &&
                    new Date(entry.vpnFirstRecord).toLocaleDateString('tr-TR') === cleanSelectedDate;

                return entryDate === cleanSelectedDate || vpnDateMatch;
            });

            // 4) Saatleri formatla (kayıt yoksa uyarı yaz)
            const officeFirst = details?.firstRecord ? new Date(details.firstRecord).toLocaleTimeString('tr-TR') : 'Kayıt yok';
            const officeLast = details?.lastRecord ? new Date(details.lastRecord).toLocaleTimeString('tr-TR') : 'Kayıt yok';
            const vpnFirst = details?.vpnFirstRecord ? new Date(details.vpnFirstRecord).toLocaleTimeString('tr-TR') : 'Kayıt yok';
            const vpnLast = details?.vpnLastRecord ? new Date(details.vpnLastRecord).toLocaleTimeString('tr-TR') : 'Kayıt yok';

            // 5) Eski modal varsa kaldır
            $('#detailsModal').remove();

            // 6) Modal HTML’ini oluştur ve sayfaya ekle
            const modalHtml = `
<div class="modal fade" id="detailsModal" tabindex="-1" aria-labelledby="detailsModalLabel" aria-hidden="true">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="detailsModalLabel">Detaylar – ${cleanSelectedDate}</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body">
        <p>Ofis Başlangıç Saati: ${officeFirst}</p>
        <p>Ofis Bitiş Saati: ${officeLast}</p>
        <p>VPN Başlangıç Saati: ${vpnFirst}</p>
        <p>VPN Bitiş Saati: ${vpnLast}</p>
        <p>Ofis Süresi: ${formattedOfficeHours} saat</p>
        <p>VPN Süresi: ${formattedVpnHours} saat</p>
        <p>Geç Kaldığı Gün Sayısı: ${lateDays}</p>
      </div>
    </div>
  </div>
</div>`;
            $('body').append(modalHtml);

            // 7) Bootstrap 5 modal nesnesini oluştur ve göster
            const modalEl = document.getElementById('detailsModal');
            new bootstrap.Modal(modalEl).show();
        })
        .catch(err => console.error('Detaylar alınamadı:', err));
}