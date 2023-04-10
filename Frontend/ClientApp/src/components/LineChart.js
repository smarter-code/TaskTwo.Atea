// LineChart.js
import React from 'react';
import { Line } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    TimeScale,
    BarElement,
    Title,
    PointElement,
    Tooltip,
    Legend,
    LineElement,
    LineController,
} from 'chart.js';

// Register the elements and plugins
ChartJS.register(
    CategoryScale,
    LinearScale,
    TimeScale,
    BarElement,
    Title,
    PointElement,
    Tooltip,
    Legend,
    LineElement,
    LineController
);
const LineChart = ({ title, options, data }) => {
    return (
        <div className="chart-container">
            <h2>{title}</h2>
            <Line data={data} options={options} />
        </div>
    );
};

export default LineChart;
