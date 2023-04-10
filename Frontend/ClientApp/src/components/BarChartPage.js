import React from 'react';
import { Bar } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    CategoryScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
} from 'chart.js';

const BarChartPage = (props) => {
    ChartJS.register(
        CategoryScale,
        BarElement,
        Title,
        Tooltip,
        Legend
    );
    const { title,options,data } = props;

    return (
        <div className="row">
            <h3 className="col text-center">{title}</h3>
            <Bar options={options} data={data} />
        </div>
    );
};

export default BarChartPage;
