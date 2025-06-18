import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import CrawlerRequest from './pages/CrawlerRequest';
import CrawlerTaskList from './pages/CrawlerTaskList'; // 匯入
import ProductHistory from './pages/ProductHistory';

export default function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<CrawlerRequest />} />
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/crawlerrequest" element={<CrawlerRequest />} />
                <Route path="/tasks" element={<CrawlerTaskList />} />
                <Route path="/product-history" element={<ProductHistory />} />
                {/* 可以在這裡添加更多路由 */}
            </Routes>
        </BrowserRouter>
    );
}
