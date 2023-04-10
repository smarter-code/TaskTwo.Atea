import React from 'react';

const WeatherContext = React.createContext({
    fetchWeatherFunction: '', // Default value
    setWeatherFunction: () => { },
});

export default WeatherContext;