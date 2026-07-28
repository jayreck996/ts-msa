import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api';
import { useAuthStore } from '../store/authStore';

export default function Auth() {
  const [mode, setMode] = useState<'login' | 'register'>('login');
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const setSession = useAuthStore(s => s.setSession);
  const navigate = useNavigate();

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const auth = mode === 'login'
        ? await api.login({ username, password })
        : await api.register({ username, email, password });
      setSession(auth);
      navigate('/quizzes');
    } catch {
      setError(mode === 'login' ? 'Invalid username or password' : 'Registration failed (username may be taken)');
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="container" style={{ maxWidth: '360px' }}>
      <div className="page-header"><h1>{mode === 'login' ? 'Log In' : 'Create Account'}</h1></div>
      <form className="card" onSubmit={submit} style={{ display: 'grid', gap: '0.75rem' }}>
        <input
          type="text" placeholder="Username" value={username}
          onChange={e => setUsername(e.target.value)} required
        />
        {mode === 'register' && (
          <input
            type="text" placeholder="Email" value={email}
            onChange={e => setEmail(e.target.value)} required
          />
        )}
        <input
          type="password" placeholder="Password" value={password}
          onChange={e => setPassword(e.target.value)} required
        />
        {error && <p style={{ color: 'var(--red)', fontSize: '0.85rem' }}>{error}</p>}
        <button type="submit" className="btn-primary" disabled={submitting} style={{ marginLeft: 0 }}>
          {submitting ? 'Please wait...' : mode === 'login' ? 'Log In' : 'Register'}
        </button>
      </form>
      <p className="stat" style={{ marginTop: '1rem' }}>
        {mode === 'login' ? "Don't have an account? " : 'Already have an account? '}
        <button
          type="button"
          className="btn-outline"
          style={{ marginLeft: '0.4rem', padding: '0.2rem 0.6rem' }}
          onClick={() => { setMode(mode === 'login' ? 'register' : 'login'); setError(''); }}
        >
          {mode === 'login' ? 'Register' : 'Log In'}
        </button>
      </p>
    </div>
  );
}
