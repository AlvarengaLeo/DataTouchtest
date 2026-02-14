/**
 * ECharts Appointments Dashboard for DataTouch
 * 3 charts: Status Distribution (donut), Daily Volume (bar), Hourly Heatmap
 * Pattern follows echarts-analytics.js
 */

window.EChartsAppointments = {
    instances: new Map(),

    /**
     * Initialize or update a chart by type
     * @param {string} elementId - DOM element ID
     * @param {string} chartType - 'status' | 'volume' | 'heatmap'
     * @param {object} data - Chart-specific data
     */
    init: function (elementId, chartType, data) {
        const container = document.getElementById(elementId);
        if (!container) {
            console.warn('EChartsAppointments: Container not found:', elementId);
            return;
        }

        this.dispose(elementId);

        const chart = echarts.init(container, 'dark');
        this.instances.set(elementId, chart);

        switch (chartType) {
            case 'status': this.setStatusOptions(chart, data); break;
            case 'volume': this.setVolumeOptions(chart, data); break;
            case 'heatmap': this.setHeatmapOptions(chart, data); break;
        }

        const resizeObserver = new ResizeObserver(() => chart.resize());
        resizeObserver.observe(container);
        container._resizeObserver = resizeObserver;
    },

    /**
     * Status Distribution — Donut chart
     * data: { labels: [], values: [], colors: [] }
     */
    setStatusOptions: function (chart, data) {
        chart.setOption({
            backgroundColor: 'transparent',
            tooltip: {
                trigger: 'item',
                backgroundColor: 'rgba(24, 24, 45, 0.95)',
                borderColor: 'rgba(52, 211, 153, 0.3)',
                borderWidth: 1,
                padding: [10, 14],
                textStyle: { color: '#fff', fontSize: 13 },
                formatter: '{b}: {c} ({d}%)'
            },
            legend: {
                bottom: 0, left: 'center',
                textStyle: { color: 'rgba(255,255,255,0.6)', fontSize: 11 },
                itemWidth: 10, itemHeight: 10, itemGap: 14
            },
            series: [{
                type: 'pie',
                radius: ['45%', '72%'],
                center: ['50%', '45%'],
                avoidLabelOverlap: true,
                itemStyle: { borderRadius: 6, borderColor: 'rgba(24,24,45,0.8)', borderWidth: 2 },
                label: { show: false },
                emphasis: {
                    label: { show: true, fontSize: 14, fontWeight: 700, color: '#fff' },
                    itemStyle: { shadowBlur: 20, shadowColor: 'rgba(52,211,153,0.3)' }
                },
                data: (data.labels || []).map((label, i) => ({
                    name: label,
                    value: (data.values || [])[i] || 0,
                    itemStyle: { color: (data.colors || [])[i] || '#6B7280' }
                }))
            }]
        });
    },

    /**
     * Daily Volume — Bar + Line chart
     * data: { dates: [], booked: [], completed: [], cancelled: [] }
     */
    setVolumeOptions: function (chart, data) {
        chart.setOption({
            backgroundColor: 'transparent',
            tooltip: {
                trigger: 'axis',
                backgroundColor: 'rgba(24, 24, 45, 0.95)',
                borderColor: 'rgba(52, 211, 153, 0.3)',
                borderWidth: 1,
                padding: [10, 14],
                textStyle: { color: '#fff', fontSize: 12 }
            },
            legend: {
                top: 0, right: 0,
                textStyle: { color: 'rgba(255,255,255,0.6)', fontSize: 11 },
                itemWidth: 12, itemHeight: 8
            },
            grid: { top: 36, right: 16, bottom: 28, left: 40, containLabel: false },
            xAxis: {
                type: 'category',
                data: (data.dates || []).map(d => {
                    const dt = new Date(d + 'T00:00:00');
                    return dt.toLocaleDateString('es', { day: '2-digit', month: 'short' });
                }),
                axisLabel: { color: 'rgba(255,255,255,0.45)', fontSize: 10, rotate: 30 },
                axisLine: { lineStyle: { color: 'rgba(255,255,255,0.08)' } },
                axisTick: { show: false }
            },
            yAxis: {
                type: 'value', minInterval: 1,
                axisLabel: { color: 'rgba(255,255,255,0.45)', fontSize: 10 },
                splitLine: { lineStyle: { color: 'rgba(255,255,255,0.06)' } }
            },
            series: [
                {
                    name: 'Agendadas', type: 'bar', stack: 'total',
                    data: data.booked || [],
                    itemStyle: { color: '#3B82F6', borderRadius: [2, 2, 0, 0] },
                    barMaxWidth: 20
                },
                {
                    name: 'Completadas', type: 'bar', stack: 'total',
                    data: data.completed || [],
                    itemStyle: { color: '#22C55E', borderRadius: [2, 2, 0, 0] },
                    barMaxWidth: 20
                },
                {
                    name: 'Canceladas', type: 'line', smooth: true,
                    data: data.cancelled || [],
                    lineStyle: { color: '#EF4444', width: 2 },
                    itemStyle: { color: '#EF4444' },
                    symbol: 'circle', symbolSize: 5,
                    areaStyle: { color: 'rgba(239,68,68,0.08)' }
                }
            ]
        });
    },

    /**
     * Hourly Heatmap — Hour × Day-of-week
     * data: { hours: [0..23], days: ['Lu','Ma','Mi','Ju','Vi','Sá','Do'], values: [[day,hour,count],...], max: number }
     */
    setHeatmapOptions: function (chart, data) {
        chart.setOption({
            backgroundColor: 'transparent',
            tooltip: {
                backgroundColor: 'rgba(24, 24, 45, 0.95)',
                borderColor: 'rgba(52, 211, 153, 0.3)',
                borderWidth: 1,
                padding: [8, 12],
                textStyle: { color: '#fff', fontSize: 12 },
                formatter: function (p) {
                    var days = data.days || [];
                    return days[p.value[0]] + ' · ' + (data.hours || [])[p.value[1]] + ':00 — ' + p.value[2] + ' cita(s)';
                }
            },
            grid: { top: 8, right: 16, bottom: 36, left: 50 },
            xAxis: {
                type: 'category',
                data: (data.hours || []).map(h => h + ':00'),
                axisLabel: { color: 'rgba(255,255,255,0.45)', fontSize: 9, interval: 1 },
                axisTick: { show: false },
                axisLine: { lineStyle: { color: 'rgba(255,255,255,0.08)' } },
                splitArea: { show: true, areaStyle: { color: ['transparent', 'rgba(255,255,255,0.02)'] } }
            },
            yAxis: {
                type: 'category',
                data: data.days || [],
                axisLabel: { color: 'rgba(255,255,255,0.5)', fontSize: 10 },
                axisTick: { show: false },
                axisLine: { lineStyle: { color: 'rgba(255,255,255,0.08)' } }
            },
            visualMap: {
                min: 0, max: data.max || 5,
                calculable: false, show: true,
                orient: 'horizontal', bottom: 0, left: 'center',
                itemWidth: 12, itemHeight: 100,
                inRange: { color: ['rgba(52,211,153,0.05)', 'rgba(52,211,153,0.3)', '#34D399', '#059669'] },
                textStyle: { color: 'rgba(255,255,255,0.45)', fontSize: 10 }
            },
            series: [{
                type: 'heatmap',
                data: data.values || [],
                label: { show: false },
                emphasis: {
                    itemStyle: { shadowBlur: 10, shadowColor: 'rgba(52,211,153,0.4)' }
                }
            }]
        });
    },

    /**
     * Update existing chart data
     */
    update: function (elementId, chartType, data) {
        const chart = this.instances.get(elementId);
        if (chart) {
            switch (chartType) {
                case 'status': this.setStatusOptions(chart, data); break;
                case 'volume': this.setVolumeOptions(chart, data); break;
                case 'heatmap': this.setHeatmapOptions(chart, data); break;
            }
        } else {
            this.init(elementId, chartType, data);
        }
    },

    /**
     * Dispose chart instance and cleanup
     */
    dispose: function (elementId) {
        const chart = this.instances.get(elementId);
        if (chart) {
            chart.dispose();
            this.instances.delete(elementId);
        }
        const container = document.getElementById(elementId);
        if (container && container._resizeObserver) {
            container._resizeObserver.disconnect();
            delete container._resizeObserver;
        }
    },

    /**
     * Dispose all instances
     */
    disposeAll: function () {
        for (const [id] of this.instances) {
            this.dispose(id);
        }
    }
};
