import { render, screen, waitFor } from '@testing-library/react';
import { vi } from 'vitest';
import Leaderboard from '../Leaderboard';
import { api } from '../../api';

vi.mock('../../api', () => ({
  api: {
    getLeaderboard: vi.fn(),
  },
}));

describe('Leaderboard', () => {
  it('shows loading initially', () => {
    vi.mocked(api.getLeaderboard).mockReturnValue(new Promise(() => {}));
    render(<Leaderboard />);
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('renders leaderboard entries', async () => {
    vi.mocked(api.getLeaderboard).mockResolvedValue([
      { id: 1, username: 'alice', totalPoints: 500, level: 5, currentStreak: 3 },
      { id: 2, username: 'bob',   totalPoints: 300, level: 3, currentStreak: 1 },
    ]);
    render(<Leaderboard />);
    await waitFor(() => expect(screen.getByText('alice')).toBeInTheDocument());
    expect(screen.getByText('bob')).toBeInTheDocument();
    expect(screen.getByText('500')).toBeInTheDocument();
  });

  it('shows empty message when no entries', async () => {
    vi.mocked(api.getLeaderboard).mockResolvedValue([]);
    render(<Leaderboard />);
    await waitFor(() => expect(screen.getByText(/no scores yet/i)).toBeInTheDocument());
  });
});
