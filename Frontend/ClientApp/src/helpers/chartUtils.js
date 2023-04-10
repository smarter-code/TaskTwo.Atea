import { useContext } from 'react';
import moment from 'moment';
import WeatherContext from '../helpers/weatherContext';




export const generateChartOptions = (apiData, dataType, onClickHandler) => {

    return {
        responsive: true,
        onClick: onClickHandler,
        plugins: {
            legend: {
                position: 'top',
            },
            title: {
                display: true,
            },
            tooltip: {
                enabled: true,
                mode: 'index',
                intersect: false,
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleFont: {
                    size: 14,
                    weight: 'bold',
                },
                bodyFont: {
                    size: 12,
                },
                callbacks: {
                    title: (context) => {
                        const index = context[0].dataIndex;
                        return `Last Update: ${moment(apiData[index][`lastUpdateTimeFor${dataType}`]).fromNow()}`;
                    },
                },
            },
        },
    };
};

export const generateChartData = (apiData, label, backgroundColor) => {

    return {
        labels: apiData.map((item) => item.id),
        datasets: [
            {
                label,
                data: apiData.map((item) => item[label]),
                backgroundColor,
            },
        ],
    };
};
