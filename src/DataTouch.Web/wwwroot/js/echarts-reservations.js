// ═══════════════════════════════════════════════════════════════
// ECharts — Reservation Dashboards
// 3 charts: Status donut, By day of week, By period
// ═══════════════════════════════════════════════════════════════

window.reservationCharts = {

    renderStatusDonut: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const chart = echarts.init(el);

        const statusColors = {
            'New': '#D97706',
            'InReview': '#3B82F6',
            'Confirmed': '#22C55E',
            'Cancelled': '#EF4444'
        };

        const option = {
            tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
            legend: { bottom: 0, textStyle: { color: '#9CA3AF', fontSize: 11 } },
            series: [{
                type: 'pie',
                radius: ['45%', '70%'],
                center: ['50%', '45%'],
                avoidLabelOverlap: true,
                itemStyle: { borderRadius: 6, borderColor: '#1F2937', borderWidth: 2 },
                label: { show: false },
                data: data.map(d => ({
                    name: d.label,
                    value: d.value,
                    itemStyle: { color: statusColors[d.label] || '#6B7280' }
                }))
            }]
        };

        chart.setOption(option);
        window.addEventListener('resize', () => chart.resize());
    },

    renderByDayOfWeek: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const chart = echarts.init(el);

        const option = {
            tooltip: { trigger: 'axis' },
            grid: { left: 40, right: 16, top: 16, bottom: 30 },
            xAxis: {
                type: 'category',
                data: data.map(d => d.label),
                axisLabel: { color: '#9CA3AF', fontSize: 10 },
                axisLine: { lineStyle: { color: '#374151' } }
            },
            yAxis: {
                type: 'value',
                minInterval: 1,
                axisLabel: { color: '#9CA3AF', fontSize: 10 },
                splitLine: { lineStyle: { color: '#1F2937' } }
            },
            series: [{
                type: 'bar',
                data: data.map(d => d.value),
                itemStyle: { color: '#D97706', borderRadius: [4, 4, 0, 0] },
                barMaxWidth: 32
            }]
        };

        chart.setOption(option);
        window.addEventListener('resize', () => chart.resize());
    },

    renderByPeriod: function (elementId, data) {
        const el = document.getElementById(elementId);
        if (!el) return;
        const chart = echarts.init(el);

        const option = {
            tooltip: { trigger: 'axis' },
            grid: { left: 40, right: 16, top: 16, bottom: 30 },
            xAxis: {
                type: 'category',
                data: data.map(d => d.label),
                axisLabel: { color: '#9CA3AF', fontSize: 9, rotate: 45 },
                axisLine: { lineStyle: { color: '#374151' } }
            },
            yAxis: {
                type: 'value',
                minInterval: 1,
                axisLabel: { color: '#9CA3AF', fontSize: 10 },
                splitLine: { lineStyle: { color: '#1F2937' } }
            },
            series: [{
                type: 'line',
                data: data.map(d => d.value),
                smooth: true,
                lineStyle: { color: '#D97706', width: 2 },
                areaStyle: { color: 'rgba(217, 119, 6, 0.15)' },
                itemStyle: { color: '#D97706' },
                symbol: 'circle',
                symbolSize: 4
            }]
        };

        chart.setOption(option);
        window.addEventListener('resize', () => chart.resize());
    }
};
