import React from 'react';
import { Link, useNavigate } from 'react-router-dom';

function Navbar() {
    const navigate = useNavigate();
    const token = localStorage.getItem('accessToken');

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        navigate('/login');
    };

    return (
        <nav className="navbar navbar-expand navbar-dark bg-dark px-3">
            <Link className="navbar-brand" to="/">
                JWT App
            </Link>
            <div className="navbar-nav ms-auto">
                {token ? (
                    <>
                        <Link className="nav-link text-white" to="/price-history">
                            商品歷史價格
                        </Link>
                        <Link className="nav-link text-white" to="/crawl-tasks">
                            爬蟲任務清單
                        </Link>
                        <button onClick={handleLogout} className="btn btn-outline-light btn-sm ms-2">
                            登出
                        </button>
                    </>
                ) : (
                    <>
                        <Link className="nav-link text-white" to="/login">
                            登入
                        </Link>
                        <Link className="nav-link text-white" to="/register">
                            註冊
                        </Link>
                    </>
                )}
            </div>
        </nav>
    );
}

export default Navbar;
