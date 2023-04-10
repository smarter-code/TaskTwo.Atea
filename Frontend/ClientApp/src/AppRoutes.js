import Home from "./components/Home";
import LineChartPage from './components/LineChartPage';
const AppRoutes = [
  {
    index: true,
    element: <Home/>
    },
    {
        path: '/temperature-trend/:labelName',
        element: <LineChartPage context="temperature" />
    },
    {
        path: '/wind-trend/:labelName',
        element: <LineChartPage context="wind" />
    }
];

export default AppRoutes;
