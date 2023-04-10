import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import BarChartPage from "./BarChartPage";
import moment from 'moment';
import '../styles/styles.css';
import { generateChartOptions, generateChartData } from '../helpers/chartUtils';


const Home = () => {
    const [loading, setLoading] = useState(true);
    const [apiData, setApiData] = useState([]);
    const navigate = useNavigate();

    const handleBarClick = (label, context) => {
        if (context === "temperature")
            navigate(`/temperature-trend/${label}`);
        if (context === "wind")
            navigate(`/wind-trend/${label}`);
    };

    const onClickHandler = (e, elements, context) => {
        if (elements.length > 0) {
            const index = elements[0].index;
            const label = apiData[index].id;
            handleBarClick(label, context);
        }
    };
    const onClickHandlerMinTemperature = (e, elements) => onClickHandler(e, elements, 'temperature');
    const onClickHandlerMaxWindSpeed = (e, elements) => onClickHandler(e, elements, 'wind');

    useEffect(() => {

        const fetchData = async () => {

            const response = await fetch("https://localhost:7068/MinTempMaxWind");
            const apiData = await response.json();
            setApiData(apiData);
            setLoading(false);
        }
        fetchData();  
    }, []);

    const minimumTemperatureChartOptions = generateChartOptions(apiData, 'MinimumTemperature', onClickHandlerMinTemperature);
    const maximumWindChartOptions = generateChartOptions(apiData, 'MaximumWindSpeed', onClickHandlerMaxWindSpeed);
    const minimumTempratureData = generateChartData(apiData, 'minimumTemperature', 'rgba(202, 239, 209, 1)');
    const maximumWindSpeedData = generateChartData(apiData, 'maximumWindSpeed', 'rgba(202, 244, 244, 1)');


    if (loading) {
        return <p>Loading...</p>;
    }

    return (
        <>
            <div className="container container-full-height">
                <div className="row row-full-height">
                    <div className="col-md-6 mb-4 col-full-height">
                        <BarChartPage title="Minimum Temperature Per City (Celsius)" options={minimumTemperatureChartOptions} data={minimumTempratureData}></BarChartPage>
                    </div>
                    <div className="col-md-6 mb-4 row-full-height col-full-height">
                        <BarChartPage title="Maximum Wind Speed Per City (m/s)" options={maximumWindChartOptions} data={maximumWindSpeedData}></BarChartPage>
                    </div>
                </div>
            </div>
        </>
    );

    };

export default Home;
