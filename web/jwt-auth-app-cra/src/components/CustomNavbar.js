import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Navbar, Nav, Container, Button } from 'react-bootstrap';

function CustomNavbar() {
    const navigate = useNavigate();
    const token = localStorage.getItem('accessToken');

    const handleLogout = () => {
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
        navigate('/login');
    };

    return (
        <Navbar bg="dark" variant="dark" expand="lg">
            <Container>
                <Navbar.Brand as={Link} to="/">
                    JWT App
                </Navbar.Brand>
                <Navbar.Toggle aria-controls="basic-navbar-nav" />
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="ms-auto">
                        {token ? (
                            <>
                                <Nav.Link as={Link} to="/price-history">
                                    商品歷史價格
                                </Nav.Link>
                                <Nav.Link as={Link} to="/crawl-tasks">
                                    爬蟲任務清單
                                </Nav.Link>
                                <Button variant="outline-light" size="sm" onClick={handleLogout} className="ms-2">
                                    登出
                                </Button>
                            </>
                        ) : (
                            <>
                                <Nav.Link as={Link} to="/login">
                                    登入
                                </Nav.Link>
                                <Nav.Link as={Link} to="/register">
                                    註冊
                                </Nav.Link>
                            </>
                        )}
                    </Nav>
                </Navbar.Collapse>
            </Container>
        </Navbar>
    );
}

export default CustomNavbar;
