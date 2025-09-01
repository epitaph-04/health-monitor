window.chartInstances = {};

window.createTimeSeriesChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                title: {
                    display: false
                },
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    displayColors: true,
                    callbacks: {
                        title: function(context) {
                            return `Time: ${context[0].label}`;
                        },
                        label: function(context) {
                            const label = context.dataset.label || '';
                            const value = context.parsed.y;
                            if (label.includes('Response Time')) {
                                return `${label}: ${value.toFixed(1)} ms`;
                            } else if (label.includes('Error Rate')) {
                                return `${label}: ${value.toFixed(2)}%`;
                            }
                            return `${label}: ${value}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Time'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Response Time (ms)'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    },
                    beginAtZero: true
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Error Rate (%)'
                    },
                    grid: {
                        drawOnChartArea: false,
                    },
                    beginAtZero: true,
                    max: 10
                }
            },
            elements: {
                line: {
                    tension: 0.4
                },
                point: {
                    radius: 3,
                    hoverRadius: 6
                }
            }
        }
    });
};

window.createDoughnutChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '70%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        },
                        generateLabels: function(chart) {
                            const data = chart.data;
                            if (data.labels.length && data.datasets.length) {
                                const dataset = data.datasets[0];
                                const total = dataset.data.reduce((sum, value) => sum + value, 0);
                                
                                return data.labels.map((label, i) => {
                                    const value = dataset.data[i];
                                    const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
                                    
                                    return {
                                        text: `${label}: ${value} (${percentage}%)`,
                                        fillStyle: dataset.backgroundColor[i],
                                        strokeStyle: dataset.backgroundColor[i],
                                        lineWidth: 0,
                                        hidden: isNaN(dataset.data[i]),
                                        index: i
                                    };
                                });
                            }
                            return [];
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    callbacks: {
                        label: function(context) {
                            const label = context.label || '';
                            const value = context.parsed;
                            const total = context.dataset.data.reduce((sum, val) => sum + val, 0);
                            const percentage = total > 0 ? ((value / total) * 100).toFixed(1) : '0.0';
                            return `${label}: ${value} services (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
};

window.createBarChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6
                }
            },
            scales: {
                x: {
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                }
            }
        }
    });
};

window.createScatterChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart if it exists
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'scatter',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    callbacks: {
                        title: function() {
                            return '';
                        },
                        label: function(context) {
                            const point = context.parsed;
                            return `Service: ${context.dataset.label}\nX: ${point.x.toFixed(2)}\nY: ${point.y.toFixed(2)}`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    position: 'bottom',
                    title: {
                        display: true,
                        text: 'Response Time (ms)'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    title: {
                        display: true,
                        text: 'Error Rate (%)'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                }
            },
            elements: {
                point: {
                    radius: 5,
                    hoverRadius: 8,
                    backgroundColor: 'rgba(59, 130, 246, 0.6)',
                    borderColor: 'rgba(59, 130, 246, 1)',
                    borderWidth: 1
                }
            }
        }
    });
};

window.createHeatmapChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart if it exists
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'scatter',
        data: {
            datasets: [{
                label: 'Performance Heatmap',
                data: data.map((value, index) => ({
                    x: index % 7,
                    y: Math.floor(index / 7),
                    v: value
                })),
                backgroundColor: function(context) {
                    const value = context.parsed.v;
                    const alpha = Math.min(value / 100, 1);
                    return `rgba(59, 130, 246, ${alpha})`;
                },
                borderColor: 'rgba(59, 130, 246, 1)',
                borderWidth: 1,
                pointRadius: 15
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    callbacks: {
                        title: function() {
                            return '';
                        },
                        label: function(context) {
                            const point = context.parsed;
                            const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
                            return `${dayNames[point.x]}, Week ${point.y + 1}: ${point.v.toFixed(1)}% uptime`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    position: 'bottom',
                    min: -0.5,
                    max: 6.5,
                    ticks: {
                        stepSize: 1,
                        callback: function(value) {
                            const days = ['S', 'M', 'T', 'W', 'T', 'F', 'S'];
                            return days[value] || '';
                        }
                    },
                    grid: {
                        display: false
                    }
                },
                y: {
                    type: 'linear',
                    min: -0.5,
                    max: 6.5,
                    ticks: {
                        stepSize: 1,
                        display: false
                    },
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
};

window.createAreaChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart if it exists
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            fill: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6
                }
            },
            scales: {
                x: {
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    },
                    title: {
                        display: true,
                        text: 'Resource Usage'
                    }
                }
            },
            elements: {
                line: {
                    tension: 0.4
                },
                point: {
                    radius: 3,
                    hoverRadius: 6
                }
            }
        }
    });
};

window.createMultiAxisChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy existing chart if it exists
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6
                }
            },
            scales: {
                x: {
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    },
                    beginAtZero: true
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    grid: {
                        drawOnChartArea: false,
                    },
                    beginAtZero: true
                },
                y2: {
                    type: 'linear',
                    display: false,
                    position: 'right',
                    beginAtZero: true
                }
            }
        }
    });
};

window.updateChartData = function(canvasId, newData) {
    const chart = window.chartInstances[canvasId];
    if (chart) {
        chart.data = newData;
        chart.update('active');
    }
};

window.destroyAllCharts = function() {
    Object.keys(window.chartInstances).forEach(canvasId => {
        if (window.chartInstances[canvasId]) {
            window.chartInstances[canvasId].destroy();
            delete window.chartInstances[canvasId];
        }
    });
};

window.addDataPoint = function(canvasId, label, dataPoints) {
    const chart = window.chartInstances[canvasId];
    if (chart) {
        chart.data.labels.push(label);
        chart.data.datasets.forEach((dataset, index) => {
            dataset.data.push(dataPoints[index]);
        });
        
        // Keep only last 50 data points for performance
        if (chart.data.labels.length > 50) {
            chart.data.labels.shift();
            chart.data.datasets.forEach(dataset => {
                dataset.data.shift();
            });
        }
        
        chart.update('none');
    }
};

window.exportChart = function(canvasId, filename) {
    const chart = window.chartInstances[canvasId];
    if (chart) {
        const url = chart.toBase64Image();
        const link = document.createElement('a');
        link.download = filename || `${canvasId}_chart.png`;
        link.href = url;
        link.click();
    }
};

window.resizeCharts = function() {
    Object.values(window.chartInstances).forEach(chart => {
        chart.resize();
    });
};

window.initializeTooltips = function() {
    // Initialize any custom tooltips or interactive elements
    const tooltipElements = document.querySelectorAll('.tooltip');
    tooltipElements.forEach(element => {
        element.addEventListener('mouseenter', function(e) {
            const title = e.target.getAttribute('title');
            if (title) {
                // Create and show custom tooltip
                const tooltip = document.createElement('div');
                tooltip.className = 'custom-tooltip';
                tooltip.textContent = title;
                tooltip.style.cssText = `
                    position: absolute;
                    background: rgba(0, 0, 0, 0.8);
                    color: white;
                    padding: 4px 8px;
                    border-radius: 4px;
                    font-size: 12px;
                    pointer-events: none;
                    z-index: 1000;
                `;
                document.body.appendChild(tooltip);
                
                const rect = e.target.getBoundingClientRect();
                tooltip.style.left = rect.left + rect.width / 2 - tooltip.offsetWidth / 2 + 'px';
                tooltip.style.top = rect.top - tooltip.offsetHeight - 8 + 'px';
            }
        });
        
        element.addEventListener('mouseleave', function() {
            const tooltip = document.querySelector('.custom-tooltip');
            if (tooltip) {
                tooltip.remove();
            }
        });
    });
};

// Health trend chart function
window.createHealthTrendChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: ${context.parsed.y.toFixed(1)}%`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    display: true,
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Health Score (%)'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    },
                    beginAtZero: true,
                    max: 100
                }
            },
            elements: {
                line: {
                    tension: 0.4
                },
                point: {
                    radius: 3,
                    hoverRadius: 6
                }
            }
        }
    });
};

// Alerts timeline chart function
window.createAlertsTimelineChart = function(canvasId, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
    }

    const ctx = canvas.getContext('2d');
    window.chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        boxWidth: 12,
                        font: {
                            size: 11
                        }
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: '#374151',
                    borderWidth: 1,
                    cornerRadius: 6,
                    callbacks: {
                        label: function(context) {
                            return `${context.dataset.label}: ${context.parsed.y} alerts`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    display: true,
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    }
                },
                y: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Number of Alerts'
                    },
                    grid: {
                        color: 'rgba(156, 163, 175, 0.1)'
                    },
                    beginAtZero: true
                }
            },
            elements: {
                line: {
                    tension: 0.4
                },
                point: {
                    radius: 3,
                    hoverRadius: 6
                }
            }
        }
    });
};

// File download function
window.downloadFile = function(filename, data) {
    const blob = new Blob([data], { type: 'application/octet-stream' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.initializeTooltips);
} else {
    window.initializeTooltips();
}