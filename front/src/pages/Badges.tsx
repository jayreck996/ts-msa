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

  if (loading) return <p>Loading...</p>;

  const earnedIds = new Set(earned.map(u => u.badgeId));

  return (
    <div>
      <h1>Badges</h1>
      <div>
        <input
          type="number"
          placeholder="Enter user ID"
          value={userId}
          onChange={e => setUserId(e.target.value)}
        />
        <button onClick={lookup}>Look up</button>
      </div>
      <ul>
        {badges.map(b => (
          <li key={b.id} style={{ opacity: earnedIds.has(b.id) ? 1 : 0.4 }}>
            <strong>{b.name}</strong>{earnedIds.has(b.id) ? ' checkmark' : ''} — {b.description}
          </li>
        ))}
      </ul>
    </div>
  );
}