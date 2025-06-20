import React from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import HomePage from './pages/HomePage';
import PrivateRoute from './routes/PrivateRoute';
import CrawlTasksPage from './pages/CrawlTasksPage';
import PriceHistoryPage from './pages/PriceHistoryPage';
import CustomNavbar from './components/CustomNavbar';

function App() {
    return (
        <BrowserRouter>
            <CustomNavbar />
            <Routes>
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route element={<PrivateRoute />}>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/crawl-tasks" element={<CrawlTasksPage />} />
                    <Route path="/price-history" element={<PriceHistoryPage />} />
                </Route>
            </Routes>
        </BrowserRouter>
    );
}

export default App;
