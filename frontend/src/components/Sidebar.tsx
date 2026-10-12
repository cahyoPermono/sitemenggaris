import React from 'react';
import { Home, History, HelpCircle } from 'lucide-react';

export type ActiveTab = 'home' | 'history' | 'help';

interface SidebarProps {
  activeTab: ActiveTab;
  onSelectTab: (tab: ActiveTab) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ activeTab, onSelectTab }) => {
  return (
    <aside className="app-sidebar">
      <button
        type="button"
        className={`nav-item ${activeTab === 'home' ? 'active' : ''}`}
        onClick={() => onSelectTab('home')}
      >
        <Home size={18} />
        <span>Home</span>
      </button>

      <button
        type="button"
        className={`nav-item ${activeTab === 'history' ? 'active' : ''}`}
        onClick={() => onSelectTab('history')}
      >
        <History size={18} />
        <span>Riwayat Konversi</span>
      </button>

      <button
        type="button"
        className={`nav-item ${activeTab === 'help' ? 'active' : ''}`}
        onClick={() => onSelectTab('help')}
      >
        <HelpCircle size={18} />
        <span>Bantuan</span>
      </button>
    </aside>
  );
};
