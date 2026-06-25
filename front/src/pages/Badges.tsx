import { useEffect, useState } from 'react';
import { api, type Badge, type UserBadge } from '../api';

export default function Badges() {
  const [badges, setBadges] = useState<Badge[]>([]);
  const [earned, setEarned] = useState<UserBadge[]>([]);
  const [userId, setUserId] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.getBadges().then(setBadges).finally(() => setLoading(false));
  }, []);

  function lookup() {
    const id = parseInt(userId);
    if (!isNaN(id)) api.getUserBadges(id).then(setEarned);
  }

  if (loading) return <div className="container"><p className="empty">Loading...</p></div>;

  const earnedIds = new Set(earned.map(u => u.badgeId));

  return (
    <div className="container">
      <div className="page-header"><h1>Badges</h1></div>
      <div className="lookup">
        <input type="number" placeholder="Enter user ID" value={userId} onChange={e => setUserId(e.target.value)} />
        <button onClick={lookup}>Look up</button>
      </div>
      {badges.length === 0 && <p className="empty">No badges yet.</p>}
      {badges.map(b => (
        <div key={b.id} className={`card ${earnedIds.has(b.id) ? '' : 'dim'}`}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
            <h2 style={{ flex: 1 }}>{b.name} {earnedIds.has(b.id) ? '✓' : ''}</h2>
            {earnedIds.has(b.id) && <span className="badge badge-purple">Earned</span>}
          </div>
          <p style={{ color: 'var(--muted)', fontSize: '0.9rem', marginTop: '0.3rem' }}>{b.description}</p>
        </div>
      ))}
    </div>
  );
}
