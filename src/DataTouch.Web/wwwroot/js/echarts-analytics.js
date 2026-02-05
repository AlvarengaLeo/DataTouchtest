/**
 * ECharts Analytics Wrapper for DataTouch Dashboard
 * Provides functions to initialize, update, and dispose ECharts instances from Blazor
 */

window.EChartsAnalytics = {
    instances: new Map(),

    /**
     * Initialize an ECharts instance with the analytics chart configuration
     * @param {string} elementId - The DOM element ID to render the chart
     * @param {object} data - Chart data { labels: [], interactions: [], leads: [], maxInteractions: number }
     */
    init: function (elementId, data) {
        const container = document.getElementById(elementId);
        if (!container) {
            console.warn('EChartsAnalytics: Container not found:', elementId);
            return;
        }

        // Dispose existing instance if any
        this.dispose(elementId);

        // Initialize ECharts
        const chart = echarts.init(container, 'dark');
        this.instances.set(elementId, chart);

        // Set options
        this.setOptions(chart, data);

        // Setup resize observer for responsiveness
        const resizeObserver = new ResizeObserver(() => {
            chart.resize();
        });
        resizeObserver.observe(container);

        // Store observer for cleanup
        container._resizeObserver = resizeObserver;
    },

    /**
     * Set chart options with the provided data
     */
    setOptions: function (chart, data) {
        const options = {
            backgroundColor: 'transparent',
            tooltip: {
                trigger: 'axis',
                axisPointer: {
                    type: 'shadow'
                },
                backgroundColor: 'rgba(24, 24, 45, 0.95)',
                borderColor: 'rgba(167, 139, 250, 0.3)',
                borderWidth: 1,
                padding: [12, 16],
                textStyle: {
                    color: '#ffffff',
                    fontSize: 13
                },
                formatter: function (params) {
                    // Enterprise tooltip with bucket data
                    const idx = params[0].dataIndex;
                    const bucket = data.buckets ? data.buckets[idx] : null;

                    let html = '';
                    if (bucket) {
                        // Title (tooltipTitle)
                        html += `<div style="font-weight:600;font-size:14px;margin-bottom:4px;">${bucket.tooltipTitle}</div>`;
                        // Range (tooltipRange)
                        html += `<div style="font-size:11px;color:#9CA3AF;margin-bottom:10px;">${bucket.tooltipRange}</div>`;
                    } else {
                        html += `<div style="font-weight:600;margin-bottom:8px;">${params[0].name}</div>`;
                    }

                    // Metrics
                    params.forEach(p => {
                        const color = p.seriesIndex === 0 ? '#A78BFA' : '#5EEAD4';
                        html += `<div style="display:flex;align-items:center;gap:8px;margin:4px 0;">
                            <span style="width:10px;height:10px;border-radius:50%;background:${color};"></span>
                            <span>${p.seriesName}: <strong>${p.value}</strong></span>
                        </div>`;
                    });

                    // Conversion rate
                    if (bucket && bucket.conversionRate !== undefined) {
                        html += `<div style="margin-top:8px;padding-top:8px;border-top:1px solid rgba(255,255,255,0.1);font-size:12px;">
                            Conversión: <strong>${bucket.conversionRate.toFixed(2)}%</strong>
                        </div>`;
                    }

                    return html;
                }
            },
            legend: {
                show: false  // External HTML pills legend instead
            },
            grid: {
                left: '6%',   // Tighter margins for fuller width
                right: '6%',
                top: '6%',
                bottom: '12%',
                containLabel: true
            },
            xAxis: {
                type: 'category',
                data: data.labels || ['L', 'M', 'M', 'J', 'V', 'S', 'D'],
                axisLine: {
                    show: false
                },
                axisTick: {
                    show: false
                },
                axisLabel: {
                    color: 'rgba(255, 255, 255, 0.7)',
                    fontSize: 12,
                    fontWeight: 600
                }
            },
            yAxis: [
                {
                    type: 'value',
                    name: '',
                    position: 'left',
                    min: 0,
                    max: (function () {
                        // Calculate max for Interacciones axis with round steps
                        const maxInt = Math.max(...(data.interactions || [0]));
                        // Determine step: 10 for small values, 20 for medium, 50 for large
                        let step = 10;
                        if (maxInt > 100) step = 20;
                        if (maxInt > 300) step = 50;
                        if (maxInt > 500) step = 100;
                        // Round max up to next step
                        return maxInt <= 0 ? 10 : Math.ceil(maxInt / step) * step;
                    })(),
                    interval: (function () {
                        const maxInt = Math.max(...(data.interactions || [0]));
                        // Aim for 5-7 labels
                        if (maxInt <= 50) return 10;
                        if (maxInt <= 100) return 20;
                        if (maxInt <= 300) return 50;
                        if (maxInt <= 500) return 100;
                        return 200;
                    })(),
                    axisLine: {
                        show: false
                    },
                    axisTick: {
                        show: false
                    },
                    splitLine: {
                        lineStyle: {
                            color: 'rgba(255, 255, 255, 0.06)'
                        }
                    },
                    axisLabel: {
                        color: 'rgba(167, 139, 250, 0.9)',
                        fontSize: 11,
                        fontWeight: 500
                    }
                },
                {
                    type: 'value',
                    name: '',
                    position: 'right',
                    min: 0,
                    max: (function () {
                        // Calculate max for Leads axis with 0.5 step increments
                        const maxLeads = Math.max(...(data.leads || [0]));
                        const step = 0.5;
                        // Round max up to next 0.5 step, minimum 2.5 for readability
                        const calculatedMax = maxLeads <= 0 ? 2.5 : Math.ceil(maxLeads / step) * step;
                        return Math.max(calculatedMax, 2.5);
                    })(),
                    interval: 0.5,
                    axisLine: {
                        show: false
                    },
                    axisTick: {
                        show: false
                    },
                    splitLine: {
                        show: false
                    },
                    axisLabel: {
                        color: 'rgba(94, 234, 212, 0.9)',
                        fontSize: 11,
                        fontWeight: 500,
                        formatter: function (value) {
                            // Format leads axis: show .0 for whole numbers, .5 for halves
                            return value % 1 === 0 ? value.toFixed(0) : value.toFixed(1);
                        }
                    }
                }
            ],
            series: [
                {
                    name: 'Interacciones',
                    type: 'bar',
                    yAxisIndex: 0,
                    barWidth: '38%',
                    barGap: '8%',
                    itemStyle: {
                        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                            { offset: 0, color: '#A78BFA' },
                            { offset: 1, color: '#8B5CF6' }
                        ]),
                        borderRadius: [6, 6, 0, 0]
                    },
                    emphasis: {
                        itemStyle: {
                            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                                { offset: 0, color: '#C4B5FD' },
                                { offset: 1, color: '#A78BFA' }
                            ])
                        }
                    },
                    data: data.interactions || []
                },
                {
                    name: 'Leads',
                    type: 'bar',
                    yAxisIndex: 1,
                    barWidth: '38%',
                    itemStyle: {
                        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                            { offset: 0, color: '#5EEAD4' },
                            { offset: 1, color: '#2DD4BF' }
                        ]),
                        borderRadius: [6, 6, 0, 0]
                    },
                    emphasis: {
                        itemStyle: {
                            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                                { offset: 0, color: '#99F6E4' },
                                { offset: 1, color: '#5EEAD4' }
                            ])
                        }
                    },
                    data: data.leads || []
                }
            ]
        };

        chart.setOption(options);
    },

    /**
     * Update chart data
     */
    update: function (elementId, data) {
        const chart = this.instances.get(elementId);
        if (chart) {
            this.setOptions(chart, data);
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
    }
};
