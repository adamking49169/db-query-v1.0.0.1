import React, { useEffect, useRef } from 'react';
import './application.css';

const ChatWithData = ({ user, previousChats = [], chatHistory = [], data = [], onChatSubmit }) => {
    const userInputRef = useRef(null);

    // Filter previous chats for current user
    const filteredChats = user && previousChats.filter(chat => chat.userIdentityId === user.id);

    // Focus textarea on initial render and whenever chatHistory changes
    useEffect(() => {
        if (userInputRef.current) {
            userInputRef.current.focus();
        }
    }, [chatHistory]);

    const handleSubmit = (e) => {
        e.preventDefault();
        const value = userInputRef.current.value;
        if (onChatSubmit) onChatSubmit(value);
        // Refocus after submit
        if (userInputRef.current) {
            userInputRef.current.focus();
        }
    };

    const handleKeyDown = (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSubmit(e);
        }
    };

    return (
        <div className="auth-bg">
            {user && (
                <div className="dropdown user-avatar-topright">
                    <a href="#" id="userDropdown" data-bs-toggle="dropdown" aria-expanded="false">
                        <img
                            src={`https://ui-avatars.com/api/?name=${user.firstName}+${user.lastName}&background=0D8ABC&color=fff`}
                            alt="Avatar"
                            className="user-avatar"
                        />
                    </a>
                    <ul className="dropdown-menu dropdown-menu-end" aria-labelledby="userDropdown">
                        <li><a className="dropdown-item" href="/account/settings">Settings</a></li>
                        <li><a className="dropdown-item" href="/account/upgrade">Upgrade Plan</a></li>
                        <li><hr className="dropdown-divider" /></li>
                        <li>
                            <form action="/account/logout" method="post" className="m-0">
                                <button type="submit" className="dropdown-item">Log out</button>
                            </form>
                        </li>
                    </ul>
                </div>
            )}

            <div className="auth-card container">
                <div className="sidebar">
                    <h3>Previous Chats</h3>
                    {filteredChats && filteredChats.length > 0 ? (
                        filteredChats.map(prev => (
                            <div key={prev.id} className="history-item">
                                {prev.title} ({new Date(prev.date).toLocaleDateString()})
                            </div>
                        ))
                    ) : (
                        <div>No previous chats available.</div>
                    )}
                </div>

                <div className="main-content">
                    <div className="chat-header">Chat With Data</div>
                    <form onSubmit={handleSubmit} className="chat-input">
                        <div className="input-container">
                            <textarea
                                id="UserInput"
                                name="UserInput"
                                placeholder="Ask a question..."
                                required
                                ref={userInputRef}
                                onKeyDown={handleKeyDown}
                            />
                            <button type="submit" className="send-button">Send</button>
                        </div>
                    </form>

                    {chatHistory && chatHistory.length > 0 && (
                        <>
                            <div className="chat-history-header">Chat History</div>
                            <ul className="chat-history">
                                {(() => {
                                    const pairs = [];
                                    for (let i = 0; i < chatHistory.length; i += 2) {
                                        pairs.unshift({ user: chatHistory[i], assistant: chatHistory[i + 1] });
                                    }
                                    return pairs.map((p, idx) => (
                                        <React.Fragment key={idx}>
                                            <li className="chat-message">
                                                <div className="avatar user">U</div>
                                                <div className="chat-bubble user-message">
                                                    <span>{p.user.content}</span>
                                                    <div className="chat-meta">{p.user.role} • {new Date(p.user.timestamp).toLocaleString()}</div>
                                                </div>
                                            </li>
                                            {p.assistant && (
                                                <li className="chat-message">
                                                    <img src="/images/navbar-brand.png" alt="User Avatar" className="avatar user" style={{ objectFit: 'cover', width: '36px', height: '36px', padding: '2px' }} />
                                                    <div className="chat-bubble assistant-message">
                                                        <span>{p.assistant.content}</span>
                                                        <div className="chat-meta">{p.assistant.role} • {new Date(p.assistant.timestamp).toLocaleString()}</div>
                                                    </div>
                                                </li>
                                            )}
                                            <li className="separator"><hr className="chat-separator" /></li>
                                        </React.Fragment>
                                    ));
                                })()}
                            </ul>
                        </>
                    )}

                    {data && data.length > 0 && (
                        <>
                            <div className="chat-history-header">Data Output</div>
                            <table className="table">
                                <thead>
                                    <tr><th>Column</th><th>Value</th></tr>
                                </thead>
                                <tbody>
                                    {data.map(row => (
                                        <tr key={row.columnName}><td>{row.columnName}</td><td>{row.value}</td></tr>
                                    ))}
                                </tbody>
                            </table>
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ChatWithData;
