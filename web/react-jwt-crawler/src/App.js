import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import CrawlerRequest from './pages/CrawlerRequest';

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<CrawlerRequest />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/crawlerrequest" element={<CrawlerRequest />} />
            </Routes>
        </BrowserRouter>
    );
}
