// LineChartPage.js
import React, { useState, useEffect, useContext } from 'react';
import { useParams } from 'react-router-dom';
import LineChart from './LineChart';
import WeatherContext from '../helpers/weatherContext';

const LineChartPage = ({ context}) => {

    console.log(context);
    const { labelName } = useParams();
    const [loading, setLoading] = useState(true);
    const [timeSeriesData, setTimeSeriesData] = useState([]);
    useEffect(() => {
       
        const fetchData = async () => {
            
            const response = await fetch(`https://localhost:7068/lasttwohoursdata?countrycity=${labelName}`);
            const timeSeriesData = await response.json();
            setTimeSeriesData(timeSeriesData);
            setLoading(false);
            setTimeSeriesData(timeSeriesData);
        };

        fetchData();
    }, [labelName]);

    const lineChartData = {
        labels: timeSeriesData.map((item) => item.lastUpdateTime),
        datasets: [
            {
                label: labelName,
                data: timeSeriesData.map((item) => context == "wind" ? item.windSpeed : item.temperature),
                borderColor: 'rgba(75,192,192,1)',
                fill: false,
            },
        ],
    };

    const lineChartOptions = {
        responsive: true,
    };
    if (loading) {
        return <p>Loading...</p>;
    }
    return (
        <div>
            <LineChart title={labelName} options={lineChartOptions} data={lineChartData}></LineChart>
        </div>
    );
};

export default LineChartPage;