google.charts.load('current', { packages: ['corechart'] });

document.addEventListener("DOMContentLoaded", function () {
    loadDepartments();
    setDefaultDates();
  

    document.getElementById("btnFilter")?.addEventListener("click", function () {
        const startDate = document.getElementById("startDate").value;
        const endDate = document.getElementById("endDate").value;
        const departmentId = document.getElementById("departmentSelect").value;
        const chartType = document.getElementById("chartType")?.value;

        fetchAndRenderStats(startDate, endDate, departmentId);
        fetchAndRenderTopEmployees(startDate, departmentId);

        if (chartType === "google") {
            document.getElementById("departmentChart").style.display = "none";
            document.getElementById("department_chart_div").style.display = "block";
            document.getElementById("chartPager").style.display = "block";
            fetchAndRenderGoogleChartPerformance(startDate, endDate, departmentId);
        } else {
            document.getElementById("departmentChart").style.display = "block";
            document.getElementById("department_chart_div").style.display = "none";
            document.getElementById("chartPager").style.display = "none";
            fetchAndRenderPerformance(startDate, endDate, departmentId);
        }
    });
});

function setDefaultDates() {
    const now = new Date();
    const past = new Date(now);
    past.setDate(now.getDate() - 30);

    // Başlangıç (30 gün önce) ve bitiş (bugün)
    document.getElementById("date-input-vpn").value = past.toISOString().split('T')[0];
    document.getElementById("date-input-vpn2").value = now.toISOString().split('T')[0];
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

// Chart.js çizimi
function fetchAndRenderPerformance(startDate, endDate, departmentId) {
    const url = `/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`;
    fetch(url)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response;
            const chartData = processChartJsData(data);
            drawChartJs(chartData);
            fillTable(chartData);
        });
}

function processChartJsData(data) {
    const grouped = {};
    data.forEach(item => {
        const key = `${item.name} ${item.surName}`;
        if (!grouped[key]) {
            grouped[key] = { fullName: key, officeHours: 0, vpnHours: 0, lateDays: 0 };
        }
        if (item.workingHour) grouped[key].officeHours += timeStringToHours(item.workingHour);
        if (item.duration) grouped[key].vpnHours += timeStringToHours(item.duration);
        const lateOffice = new Date(item.firstRecord).getHours() + (new Date(item.firstRecord).getMinutes() / 60);
        const lateVpn = new Date(item.vpnFirstRecord).getHours() + (new Date(item.vpnFirstRecord).getMinutes() / 60);
        if (lateOffice > 8.5 || lateVpn > 8.5) grouped[key].lateDays++;
    });

    return Object.values(grouped);
}

function drawChartJs(chartData) {
    const ctx = document.getElementById("departmentChart").getContext("2d");
    const labels = chartData.map(x => x.fullName);
    const office = chartData.map(x => x.officeHours.toFixed(2));
    const vpn = chartData.map(x => x.vpnHours.toFixed(2));

    if (window.performanceChart) window.performanceChart.destroy();

    window.performanceChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [
                { label: 'Ofis Saat', data: office, backgroundColor: '#4CAF50' },
                { label: 'VPN Saat', data: vpn, backgroundColor: '#2196F3' }
            ]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'top' },
                title: { display: true, text: 'Çalışanların Ofis & VPN Süreleri' }
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
            <td>${item.lateDays}</td>`;
        tbody.appendChild(row);
    });
}

function fetchAndRenderStats(startDate, endDate, departmentId) {
    fetch(`/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`)
        .then(res => res.json())
        .then(response => {
            const data = response.data || response;
            const unique = new Set();
            let total = 0, visits = 0;
            data.forEach(e => {
                const h = timeStringToHours(e.toplamZaman || e.workingHour || "0:00:00");
                total += h;
                if (h > 1) visits++;
                unique.add(e.remoteEmployeeId);
            });
            document.getElementById('GetCustomerAmount').innerText = unique.size;
            document.getElementById('GetCustomerAmountByStatutrue').innerText = total.toFixed(2);
            document.getElementById('GetDiscountAmount').innerText = (total / (visits || 1)).toFixed(2);
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
            data.forEach(item => {
                const total = convertDay(item.averageHour);
                const weekday = parseDuration(item.totalHour);
                const weekend = parseDuration(item.weekendTotalHour);
                const row = document.createElement("tr");
                row.innerHTML = `
                    <td>${item.name}</td>
                    <td>${total.toFixed(2)}</td>
                    <td>${weekday.toFixed(2)}</td>
                    <td>${weekend.toFixed(2)}</td>`;
                tbody.appendChild(row);
            });
        });
}

// ================= GOOGLE CHART ===============
//google.charts.load('current', { packages: ['corechart'] });

//document.addEventListener("DOMContentLoaded", function () {
//    loadDepartments();
//    setDefaultDates();
//    document.getElementById("chartType").value = "google";

//    document.getElementById("btnFilter")?.addEventListener("click", function () {
//        const startDate = document.getElementById("startDate").value;
//        const endDate = document.getElementById("endDate").value;
//        const departmentId = document.getElementById("departmentSelect").value;
//        const chartType = document.getElementById("chartType")?.value;

//        fetchAndRenderStats(startDate, endDate, departmentId);
//        fetchAndRenderTopEmployees(startDate, departmentId);

//        if (chartType === "google") {
//            document.getElementById("departmentChart").style.display = "none";
//            document.getElementById("department_chart_div").style.display = "block";
//            document.getElementById("chartPager").style.display = "block";
//            fetchAndRenderGoogleChartPerformance(startDate, endDate, departmentId);
//        } else {
//            document.getElementById("departmentChart").style.display = "block";
//            document.getElementById("department_chart_div").style.display = "none";
//            document.getElementById("chartPager").style.display = "none";
//            fetchAndRenderPerformance(startDate, endDate, departmentId);
//        }
//    });
//});

//function setDefaultDates() {
//    const now = new Date();
//    const past = new Date();
//    past.setDate(now.getDate() - 30);
//    document.getElementById("startDate").value = past.toISOString().split('T')[0];
//    document.getElementById("endDate").value = now.toISOString().split('T')[0];
//}

//function loadDepartments() {
//    fetch("/PersonnalTracking/GetDepartments")
//        .then(res => res.json())
//        .then(data => {
//            const select = document.getElementById("departmentSelect");
//            data.data.forEach(dept => {
//                const option = document.createElement("option");
//                option.value = dept.id;
//                option.textContent = dept.name;
//                select.appendChild(option);
//            });
//        });
//}

//function fetchAndRenderStats(startDate, endDate, departmentId) {
//    fetch(`/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`)
//        .then(res => res.json())
//        .then(response => {
//            const data = response.data || response;
//            const unique = new Set();
//            let total = 0, visits = 0;
//            data.forEach(e => {
//                const h = timeStringToHours(e.toplamZaman || e.workingHour || "0:00:00");
//                total += h;
//                if (h > 1) visits++;
//                unique.add(e.remoteEmployeeId);
//            });
//            document.getElementById('GetCustomerAmount').innerText = unique.size;
//            document.getElementById('GetCustomerAmountByStatutrue').innerText = total.toFixed(2);
//            document.getElementById('GetDiscountAmount').innerText = (total / (visits || 1)).toFixed(2);
//        });
//}

//function fetchAndRenderTopEmployees(startDate, departmentId) {
//    const date = new Date(startDate);
//    const month = date.getMonth() + 1;
//    const year = date.getFullYear();
//    fetch(`/PersonnalTracking/BestPersonalMonth?month=${month}&year=${year}&departmentId=${departmentId}`)
//        .then(res => res.json())
//        .then(response => {
//            const data = response.data || response;
//            const tbody = document.querySelector("#employeeTable tbody");
//            tbody.innerHTML = "";
//            data.forEach(item => {
//                const total = convertDay(item.averageHour);
//                const weekday = parseDuration(item.totalHour);
//                const weekend = parseDuration(item.weekendTotalHour);
//                const row = document.createElement("tr");
//                row.innerHTML = `
//                    <td>${item.name}</td>
//                    <td>${total.toFixed(2)}</td>
//                    <td>${weekday.toFixed(2)}</td>
//                    <td>${weekend.toFixed(2)}</td>`;
//                tbody.appendChild(row);
//            });
//        });
//}

//function fetchAndRenderGoogleChartPerformance(startDate, endDate, departmentId) {
//    fetch(`/PersonnalTracking/GetAllEmployeesWithParams?startDate=${startDate}&endDate=${endDate}&departmentId=${departmentId}`)
//        .then(res => res.json())
//        .then(response => {
//            const data = response.data || response;
//            processChartDataGoogle(data, startDate, endDate);
//        });
//}

//function processChartDataGoogle(data, startDate, endDate) {
//    const lateMap = new Map();
//    const grouped = new Map();

//    data.forEach(entry => {
//        const fullName = entry.name + " " + entry.surName;
//        const record = grouped.get(fullName) || {
//            remoteEmployeeId: entry.remoteEmployeeId,
//            officeHours: [],
//            vpnHours: [],
//            dates: new Set(),
//            officeDates: new Set(),
//            vpnDates: new Set()
//        };

//        if (entry.workingHour) record.officeHours.push(timeStringToHours(entry.workingHour));
//        if (entry.duration) record.vpnHours.push(timeStringToHours(entry.duration));
//        if (entry.date) record.dates.add(entry.date);
//        if (entry.vpnDate) record.dates.add(entry.vpnDate);
//        if (entry.firstRecord) record.officeDates.add(entry.date);
//        if (entry.vpnFirstRecord) record.vpnDates.add(entry.vpnDate);

//        const lateOffice = entry.firstRecord ? new Date(entry.firstRecord).getHours() + (new Date(entry.firstRecord).getMinutes() / 60) : 0;
//        const lateVpn = entry.vpnFirstRecord ? new Date(entry.vpnFirstRecord).getHours() + (new Date(entry.vpnFirstRecord).getMinutes() / 60) : 0;
//        if (lateOffice > 8.5 || lateVpn > 8.5) {
//            lateMap.set(fullName, (lateMap.get(fullName) || 0) + 1);
//        }

//        grouped.set(fullName, record);
//    });

//    const chartData = [];
//    for (const [name, r] of grouped.entries()) {
//        const totalDays = r.dates.size;
//        const officeDays = r.officeDates.size;
//        const vpnDays = totalDays - officeDays;
//        chartData.push({
//            remoteEmployeeId: r.remoteEmployeeId,
//            employeeName: `${name} (TÇG: ${totalDays} gün)`,
//            officeHours: safeNumber(average(r.officeHours)),
//            vpnHours: safeNumber(average(r.vpnHours)),
//            lateDays: safeNumber(lateMap.get(name) || 0),
//            officeDays,
//            vpnDays
//        });
//    }

//    chartData.sort((a, b) => (b.officeHours + b.vpnHours) - (a.officeHours + a.vpnHours));

//    const dt = new google.visualization.DataTable();
//    dt.addColumn('string', 'Employee');
//    dt.addColumn('number', 'Ofis Saat');
//    dt.addColumn({ type: 'string', role: 'tooltip' });
//    dt.addColumn('number', 'VPN Saat');
//    dt.addColumn({ type: 'string', role: 'tooltip' });
//    dt.addColumn('number', 'Geç Kalma');
//    dt.addColumn({ type: 'string', role: 'tooltip' });

//    chartData.forEach(e => {
//        const row = [
//            String(e.employeeName),
//            safeNumber(e.officeHours),
//            `Ofis: ${safeNumber(e.officeHours).toFixed(2)} saat\nGün: ${e.officeDays}`,
//            safeNumber(e.vpnHours),
//            `VPN: ${safeNumber(e.vpnHours).toFixed(2)} saat\nGün: ${e.vpnDays}`,
//            safeNumber(e.lateDays),
//            `Geç Kaldığı Gün: ${e.lateDays}`
//        ];

//        if (
//            typeof row[1] === 'number' &&
//            typeof row[3] === 'number' &&
//            typeof row[5] === 'number'
//        ) {
//            dt.addRow(row);
//        } else {
//            console.warn("Satır atlandı (veri tipi hatası):", row);
//        }
//    });

//    const options = {
//        title: 'Google Chart: Ofis/VPN ve Geç Kalma',
//        isStacked: true,
//        hAxis: { title: 'Saat' },
//        vAxis: { title: 'Personel' },
//        tooltip: { isHtml: true },
//        bar: { groupWidth: '50%' },
//        chartArea: { width: '70%', height: '70%' },
//        colors: ['#28a745', '#17a2b8', '#dc3545']
//    };

//    const chart = new google.visualization.BarChart(document.getElementById('department_chart_div'));
//    EnablePagination(chart, dt, options, 20, document.getElementById('prevButton'), document.getElementById('nextButton'));
//}

//function average(arr) {
//    const total = arr.reduce((sum, val) => sum + (isNaN(val) ? 0 : val), 0);
//    const count = arr.filter(x => !isNaN(x)).length;
//    return count > 0 ? total / count : 0;
//}

//function safeNumber(value) {
//    const num = Number(value);
//    return (typeof num === "number" && !isNaN(num) && isFinite(num)) ? num : 0;
//}

//function timeStringToHours(str) {
//    if (!str || !str.includes(":")) return 0;
//    const [h, m, s] = str.split(":").map(Number);
//    return h + m / 60 + s / 3600;
//}

//function parseDuration(str) {
//    const [h, m, s] = str.split(":").map(Number);
//    return h + m / 60 + s / 3600;
//}

//function convertDay(str) {
//    if (!str || !str.includes('.')) return parseDuration(str);
//    const [d, t] = str.split('.');
//    const [h, m, s] = t.split(":").map(Number);
//    return (+d * 24) + h + m / 60 + s / 3600;
//}

//function EnablePagination(chart, dataTable, options, pageSize, prevBtn, nextBtn) {
//    let currentPage = 0;
//    const totalRows = dataTable.getNumberOfRows();

//    function drawPage() {
//        const start = currentPage * pageSize;
//        const end = Math.min(start + pageSize, totalRows);

//        const pageTable = new google.visualization.DataTable();
//        for (let i = 0; i < dataTable.getNumberOfColumns(); i++) {
//            pageTable.addColumn(dataTable.getColumnType(i), dataTable.getColumnLabel(i));
//        }

//        for (let i = start; i < end; i++) {
//            const row = [];
//            for (let j = 0; j < dataTable.getNumberOfColumns(); j++) {
//                row.push(dataTable.getValue(i, j));
//            }
//            pageTable.addRow(row);
//        }

//        chart.draw(pageTable, options);
//    }

//    prevBtn.addEventListener("click", () => {
//        if (currentPage > 0) {
//            currentPage--;
//            drawPage();
//        }
//    });

//    nextBtn.addEventListener("click", () => {
//        if ((currentPage + 1) * pageSize < totalRows) {
//            currentPage++;
//            drawPage();
//        }
//    });

//    drawPage();
//}
