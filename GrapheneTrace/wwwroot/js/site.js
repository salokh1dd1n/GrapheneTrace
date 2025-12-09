// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
document.addEventListener("DOMContentLoaded", function () {

    var canvas = document.getElementById("peakPressureChart");
    if (!canvas) return; // Page does not contain chart

    var labels = JSON.parse(canvas.dataset.labels || "[]");
    var peakData = JSON.parse(canvas.dataset.peak || "[]");

    if (labels.length === 0 || peakData.length === 0) {
        console.warn("No chart data found.");
        return;
    }

    var ctx = canvas.getContext("2d");

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Peak Pressure (mmHg)',
                data: peakData,
                fill: false,
                borderColor: 'rgb(75, 192, 192)',
                borderWidth: 2,
                tension: 0.2,
                pointRadius: 2
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    title: {
                        display: true,
                        text: 'mmHg'
                    },
                    min: 0,
                    max: 200
                },
                x: {
                    title: {
                        display: true,
                        text: 'Time'
                    }
                }
            }
        }
    });
});
// Write your JavaScript code.
