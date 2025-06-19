import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../utils/api';

function RegisterPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [confirm, setConfirm] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();

    const handleRegister = async (e) => {
        e.preventDefault();
        setError('');

        if (password !== confirm) {
            setError('密碼與確認密碼不一致');
            return;
        }

        try {
            await api.post('/auth/register', { username, password });
            alert('註冊成功，請登入');
            navigate('/login');
        } catch (err) {
            setError(err.response?.data?.message || '註冊失敗');
        }
    };

    return (
        <div className="container mt-5" style={{ maxWidth: '400px' }}>
            <h2 className="mb-4">📝 註冊帳號</h2>
            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={handleRegister}>
                <div className="mb-3">
                    <label className="form-label">帳號</label>
                    <input
                        className="form-control"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>

                <div className="mb-3">
                    <label className="form-label">密碼</label>
                    <input
                        type="password"
                        className="form-control"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>

                <div className="mb-3">
                    <label className="form-label">確認密碼</label>
                    <input
                        type="password"
                        className="form-control"
                        value={confirm}
                        onChange={(e) => setConfirm(e.target.value)}
                        required
                    />
                </div>

                <button type="submit" className="btn btn-success w-100">
                    註冊
                </button>
            </form>
        </div>
    );
}

export default RegisterPage;
