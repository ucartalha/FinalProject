
@section Breadcrumbs{Personel Takip Sistemi / Grafikler / Kişi Bazlı Performans }
@{
    ViewBag.Title = "Kişi Bazlı Performans";
}

@{
    DateTime startDate = ViewBag.StartDate ?? (DateTime.Now).AddMonths(-1);
    DateTime endDate = ViewBag.EndDate ?? DateTime.Now;
    var startdate = startDate.ToString("yyyy-MM-dd");
    var enddate = endDate.ToString("yyyy-MM-dd");
    var employeeId = ViewBag.EmployeeId;
    var employees = ViewBag.Model;
}

<style>
    .dropdown {
        display: block;
    }

        .dropdown.show {
            display: block;
        }
</style>
<link href="~/Content/libs/css/personnalTracking/TrackingStyle.css" rel="stylesheet" />
<div class="row p-1">
    <div class="panel-group m-b-0">
        <div class="panel shadow-light-5 border-radius border-none">
            <div class="panel-collapse collapse in">
                <div class="panel-body p-1">
                    <div class="row p-0 m-t-2 m-l-0 m-r-0 m-b-0">
                        <div id="collapse1" class="panel-collapse collapse in">
                            <div class="row" id="inputArea">

                                <div class="form-group col-md-2">

                                    <div class="dropdown">
                                        <label class="label-custom">Çalışanlar</label>
                                        <input id="employee-search" type="text" class="form-control" placeholder="Çalışan Ara">

                                        <ul class="dropdown-menu user-dropdown" id="userDropdown3" style="max-height: 200px; overflow-y: auto;">
                                            <li><a class="dropdown-item" href="javascript:void(0)">Loading...</a></li>
                                        </ul>
                                    </div>

                                </div>

                                <div class="form-group col-md-3">
                                    <label class="label-custom">Başlangıç Tarihi</label>
                                    <input id="date-input-vpn" class="form-control input-md BaslangicTarihi" type="date">
                                </div>
                                <div class="form-group col-md-3">
                                    <label class="label-custom">Bitiş Tarihi</label>
                                    <input id="date-input-vpn2" class="form-control input-md BitisTarihi" type="date">
                                </div>
                                <div class="form-group m-t-3">
                                    <button id="submitButton-Vpn" class="btn btn-success">
                                        <i class="fa-solid fa-magnifying-glass"></i>
                                    </button>
                                </div>
                            </div>

                        </div>
                    </div>
                </div>
            </div>


        </div>
    </div>
</div>


<div class="row">
    <div class="col-md-12">
        <div class="panel panel-default">
            <div class="panel-heading ">

                <i class="fa fa-bar-chart-o fa-fw"></i> Günlere Göre Ofis/Vpn Sayısı


                <button id="fetchDataBtn" class="btn btn-primary">Detayları Getir</button>


            </div>
            <!-- /.panel-heading -->
            <div class="panel-body">

                <div id="dailyemployee_chart_div"></div>

            </div>
            <!-- /.panel-body -->
        </div>
        <!-- /.panel -->
    </div>
</div>


<div class="row">
    <div class="col-md-12">
        <div class="panel panel-default">
            <div class="panel-heading">
                <i class="fa fa-bar-chart-o fa-fw"></i>  Tarih Aralığına Ait Ofis-VPN Grafiği
            </div>
            <!-- /.panel-heading -->
            <div class="panel-body">
                <div class="tab-content" id="myTabContent">
                    <h4 class="card-title text-center font-weight-bold"></h4>
                    <label id="selectedWeekRange" class="week-range-label"></label>
                    <div id="chart-container">
                        <div class="col-md-6">
                            <div class=" mg-b-15 font-weight-bold" style="text-align: center;">Ofis Verileri</div>
                            <div id="chart-tooltip">
                                <div class="ht-200 ht-lg-250"><canvas id="chartBar1"></canvas></div>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class=" mg-b-15 font-weight-bold" style="text-align: center;">VPN Verileri</div>
                            <div id="chart-tooltip"></div>
                            <div class="ht-200 ht-lg-250"><canvas id="chartBar2"></canvas></div>
                        </div>
                    </div>
                </div>

            </div>

        </div>
        @*/.panel-body*@
    </div>
    @*/.panel*@
</div>
<div class="modal fade" id="lateDaysModal" tabindex="-1" role="dialog" aria-labelledby="lateDaysModalLabel" aria-hidden="true">
    <div class="modal-dialog" role="document">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="lateDaysModalLabel">Geç Kalan Günler</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <p id="lateDaysEmployeeName"></p>
                <p id="lateDaysCount"></p>
                <ul id="lateDaysList" class="list-group">
                    <!-- Late days records will be appended here dynamically -->
                </ul>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Kapat</button>
            </div>
        </div>
    </div>
</div>
<!--Modal -->

<div class="modal fade" id="dataModal" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="exampleModalLabel">Veri Tablosu</h5>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="modal-body">
                <table class="table table-bordered" id="dataTable">
                    <thead>
                        <tr>

                            <th>Log Tarihi</th>
                            <th>Ad</th>
                            <th>Soyad</th>
                            <th>Grup</th>
                            <th>Giden Bayt</th>
                            <th>Gelen Bayt</th>
                            <th>Süre</th>
                            <th>İlk Kayıt</th>
                            <th>Son Kayıt</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Veriler buraya eklenecek -->
                    </tbody>
                </table>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Kapat</button>
            </div>
        </div>
    </div>
</div>

<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script src="~/Content/libs/personnalTrackingjs/tracking-Chart.js"></script>
<script src="https://www.gstatic.com/charts/loader.js"></script>


<script>
    function showLateDaysModal(employeeName, lateDays, lateRecords) {
        $('#employeeName').text(employeeName);
        $('#lateDays').text(lateDays);
        var lateRecordsHtml = lateRecords.map(record => `<li>${record}</li>`).join('');
        $('#lateRecords').html(lateRecordsHtml);
        $('#lateDaysModal').modal('show');
    }
</script>

<script>
   window.onload = function() {
    var startdate = '@startdate';
    var enddate = '@enddate';

    document.getElementById('date-input-vpn').value = startdate;
    document.getElementById('date-input-vpn2').value = enddate;

};
</script>



<script>
    function capitalizeFirstLetter(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
    }
    document.addEventListener("DOMContentLoaded", function () {
        const dropdowns = [
            { id: "userDropdown3", searchInputId: "employee-search" },
        ];

        dropdowns.forEach((dropdownData) => {
            const userDropdown = document.getElementById(dropdownData.id);
            const employeeSearch = document.getElementById(dropdownData.searchInputId);
           let overshiftId = @Html.Raw(JsonConvert.SerializeObject(ViewBag.EmployeeId ?? 0));
            let dropdownToggled = false;

            function fetchData() {
                // Your existing fetchData logic here
            }

            function updateDropdown() {
                fetch('/PersonnalTracking/GetAllEmployee')
                    .then(response => response.json())
                    .then(data => {
                        if (data && Array.isArray(data.data)) {
                            let employeeData = data.data.map(employee => {
                                let fullName = capitalizeFirstLetter(employee.firstName) + " " + capitalizeFirstLetter(employee.lastName);
                                return { fullName, ...employee };
                            });

                            // Çalışan verilerini tam ada göre sırala
                            employeeData.sort((a, b) => a.fullName.localeCompare(b.fullName));
                            userDropdown.innerHTML = "";
                            var searchQuery = employeeSearch.value.toLowerCase();

                            employeeData.forEach(employee => {
                                if (employee.fullName.toLowerCase().includes(searchQuery)) {
                                    var employeeItem = document.createElement("li");
                                    var employeeLink = document.createElement("a");
                                    employeeLink.classList.add("dropdown-item");
                                    employeeLink.href = "javascript:void(0)";
                                    employeeLink.textContent = employee.fullName;
                                    employeeLink.setAttribute("data-id", employee.id);

                                    if (employee.id === overshiftId) {
                                        employeeLink.classList.add("active");
                                        employeeSearch.value = employee.fullName;
                                            employeeSearch.click();

                                        setTimeout(() => {
                                            const firstEmployeeLink = userDropdown.querySelector(".dropdown-item");
                                            if (firstEmployeeLink) {
                                                firstEmployeeLink.click();
                                            }
                                        }, 60);
                                    }

                                    employeeLink.addEventListener("click", function () {
                                        var employees = userDropdown.querySelectorAll(".dropdown-item");
                                        employees.forEach(employee => {
                                            employee.classList.remove("active");
                                        });

                                        this.classList.add("active");
                                        overshiftId = this.getAttribute("data-id");

                                        employeeSearch.value = employee.fullName;
                                        fetchData();

                                        userDropdown.classList.remove("show");
                                        dropdownToggled = false;
                                    });

                                    employeeItem.appendChild(employeeLink);
                                    userDropdown.appendChild(employeeItem);
                                }
                            });
                        } else {
                            console.error("Invalid data format:", data);
                            userDropdown.innerHTML = "<li><a class='dropdown-item' href='javascript:void(0)'>Error loading data</a></li>";
                        }
                    })
                    .catch(error => {
                        console.error("An error occurred:", error);
                        userDropdown.innerHTML = "<li><a class='dropdown-item' href='javascript:void(0)'>Error loading data</a></li>";
                    });
            }

            userDropdown.addEventListener("click", function (event) {
                if (!dropdownToggled) {
                    userDropdown.classList.add("show");
                    dropdownToggled = true;
                } else {
                    userDropdown.classList.remove("show");
                    dropdownToggled = false;
                }
            });

            document.addEventListener("click", function (event) {
                if (dropdownToggled && !userDropdown.contains(event.target)) {
                    userDropdown.classList.remove("show");
                    dropdownToggled = false;
                }
            });

            employeeSearch.addEventListener("input", function () {
                userDropdown.classList.add("show");
                updateDropdown();
            });

            employeeSearch.addEventListener("click", function () {
                userDropdown.classList.add("show");
                updateDropdown();
            });

            userDropdown.addEventListener("click", function () {
                if (event.target.classList.contains("dropdown-item")) {
                    userDropdown.classList.remove("show");
                    dropdownToggled = false;
                }
            });

            document.addEventListener("click", function (event) {
                if (dropdownToggled && !userDropdown.contains(event.target)) {
                    userDropdown.classList.remove("show");
                    dropdownToggled = false;
                }
            });

            updateDropdown();
        });
    });
</script>

<script>

    var selectedUserId = null;
    var selectedEmployeeName = null;

    $("#userDropdown3").on("click", ".dropdown-item", function (event) {
        var target = $(this); // Tıklanan öğeyi referans al
        $("#userDropdown3 .dropdown-item").removeClass("active");
        target.addClass("active");
        // Tıklanan öğenin üst öğesindeki data-id değerini al
        selectedUserId = parseInt(target.closest('.dropdown').attr("data-id"));
        selectedEmployeeName = target.text().trim();
        $('#userDropdown3').text(selectedEmployeeName); // Buton metnini güncelle
    });

    $('#fetchDataBtn').on('click', function () {
        var startDate = document.getElementById('date-input-vpn').value;
        var endDate = document.getElementById('date-input-vpn2').value;
        if ($.fn.DataTable.isDataTable('#dataTable')) {
            $('#dataTable').DataTable().clear().destroy();
        }
        if (!selectedUserId) {
            alert('Lütfen bir kullanıcı seçin.');
            return;
        }

        if (!startDate || !endDate) {
            alert('Lütfen tarih aralığını seçin.');
            return;
        }

        $.ajax({
            url: '/PersonnalTracking/GetAllDetails',
            method: 'GET',
            data: {
                startDate: startDate,
                endDate: endDate,
                empId: selectedUserId
            },
            success: function (response) {
                // Modal içindeki tabloya verileri ekleyin
                let tableBody = $('#dataTable tbody');
                tableBody.empty(); // Önceki verileri temizle

                response.data.forEach(function (item) {
                    let row = `<tr>
                    <td>${new Date(item.logDate).toLocaleString().split(" ")[0]}</td>
                    <td>${item.firstName}</td>
                    <td>${item.lastName}</td>
                    <td>${item.group}</td>
                    <td>${item.bytesout}</td>
                    <td>${item.bytesin}</td>
                    <td>${secondsToTime(item.duration)}</td>
                    <td>${new Date(item.firstRecord).toLocaleString()}</td>
                    <td>${new Date(item.lastRecord).toLocaleString()}</td>
                </tr>`;
                    tableBody.append(row);
                });

                $('#dataTable').DataTable({
                    language: {
                        url: '/Content/Turkish.json'  // Türkçe dil dosyasının yolu
                    },
                    dom: 'Bfrtip',
                    buttons: [
                        {
                            extend: 'excelHtml5',
                            filename: "Calisanlar ",
                            title: null,
                            autoFilter: true,
                            className: 'btn btn-dark-green-1 text-white',
                            text: 'Rapor Kaydet',
                            customizeData: function (data) {
                                for (var i = 0; i < data.body.length; i++) {
                                    for (var j = 0; j < data.body[i].length; j++) {
                                        data.body[i][j] = '\0' + data.body[i][j];
                                    }
                                }
                            },
                            action: function (e, dt, node, config) {


                                ShowLoading();


                                var that = this;


                                setTimeout(function () {
                                    $.fn.dataTable.ext.buttons.excelHtml5.action.call(that, e, dt, node, config);


                                    HideLoading();


                                }, 500);
                            }
                        }
                    ]
                });
                // Modal'ı göster
                $('#dataModal').modal('show');
            },
            error: function () {
                alert('Veri alınırken bir hata oluştu.');
            }
        });
    });

</script>
<script>
    function secondsToTime(seconds) {
        var hours = Math.floor(seconds / 3600);
        var minutes = Math.floor((seconds % 3600) / 60);
        // Saat ve dakika değerlerini string olarak formatlayarak döndürme
        return hours.toString().padStart(2, '0') + ':' + minutes.toString().padStart(2, '0');
    }

    // Örnek kullanım
    var totalSeconds = 45023; // Örneğin, 12:30:23 saniye cinsinden
    var timeString = secondsToTime(totalSeconds);
    console.log(timeString); // Çıktı: "12:30"
</script>
<script src="~/Content/libs/personnalTrackingjs/dailyemployeechart.js"></script>
<script src="~/Content/libs/personnalTrackingjs/employeeChart.js"></script>



