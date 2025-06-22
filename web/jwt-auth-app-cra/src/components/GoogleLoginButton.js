import { GoogleLogin } from '@react-oauth/google';
import api, { setAuthToken } from '../utils/api';
import { useNavigate } from 'react-router-dom';

export default function GoogleLoginButton() {
    const navigate = useNavigate();

    const onSuccess = async (credentialResponse) => {
        const idToken = credentialResponse.credential;

        try {
            const res = await api.post('/auth/google-login', { googleToken: idToken });
            const { accessToken, refreshToken } = res.data;
            setAuthToken(accessToken, refreshToken);
            navigate('/');
        } catch (err) {
            console.error('登入失敗', err);
        }
    };

    return <GoogleLogin onSuccess={onSuccess} onError={() => console.log('Google 登入失敗')} useOneTap />;
}
