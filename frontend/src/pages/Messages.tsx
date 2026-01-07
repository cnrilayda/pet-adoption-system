import { useState, useEffect } from 'react';
import { MessageCircle, Send } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import api from '../lib/api';
import Card from '../components/ui/Card';
import Button from '../components/ui/Button';
import Input from '../components/ui/Input';
import { useAuth } from '../contexts/AuthContext';

interface Conversation {
  applicationId: string;
  listingTitle: string;
  otherUserName: string;
  lastMessage: string;
  unreadCount: number;
  lastMessageTime?: string;
}

interface Message {
  id: string;
  content: string;
  senderName: string;
  senderId: string;
  receiverId: string;
  sentAt: string;
  isRead: boolean;
}

export default function Messages() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [selectedConversation, setSelectedConversation] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [newMessage, setNewMessage] = useState('');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (user) {
      fetchConversations();
    } else {
      navigate('/login');
    }
  }, [user, navigate]);

  useEffect(() => {
    if (selectedConversation) {
      fetchMessages(selectedConversation);
      const interval = setInterval(() => {
        fetchMessages(selectedConversation);
      }, 5000);
      return () => clearInterval(interval);
    }
  }, [selectedConversation]);

  const fetchConversations = async () => {
    try {
      setIsLoading(true);
      const applicationsResponse = await api.get('/applications/my-applications');
      const applications = applicationsResponse.data || [];

      const conversationsList: Conversation[] = await Promise.all(
        applications.map(async (app: any) => {
          try {
            const messagesResponse = await api.get(`/messages/conversation/${app.id}`);
            const conversationMessages = messagesResponse.data || [];
            
            const lastMessageObj = conversationMessages.length > 0 
              ? conversationMessages[conversationMessages.length - 1]
              : null;

            const unreadCount = conversationMessages.filter((m: any) => 
              m.receiverId === user?.id && !m.isRead
            ).length;

            const otherUserName = app.ownerName || 'Kullanıcı';

            return {
              applicationId: app.id,
              listingTitle: app.listingTitle || 'İlan',
              otherUserName: otherUserName,
              lastMessage: lastMessageObj?.content || '',
              unreadCount: unreadCount,
              lastMessageTime: lastMessageObj?.createdAt
            };
          } catch (error) {
            return {
              applicationId: app.id,
              listingTitle: app.listingTitle || 'İlan',
              otherUserName: app.ownerName || 'Kullanıcı',
              lastMessage: '',
              unreadCount: 0
            };
          }
        })
      );

      conversationsList.sort((a, b) => {
        if (!a.lastMessageTime && !b.lastMessageTime) return 0;
        if (!a.lastMessageTime) return 1;
        if (!b.lastMessageTime) return -1;
        return new Date(b.lastMessageTime).getTime() - new Date(a.lastMessageTime).getTime();
      });

      setConversations(conversationsList);
    } catch (error) {
    } finally {
      setIsLoading(false);
    }
  };

  const fetchMessages = async (applicationId: string) => {
    try {
      const response = await api.get(`/messages/conversation/${applicationId}`);
      const messagesData = response.data || [];
      
      const formattedMessages: Message[] = messagesData.map((m: any) => ({
        id: m.id,
        content: m.content,
        senderName: m.senderName || 'Kullanıcı',
        senderId: m.senderId,
        receiverId: m.receiverId,
        sentAt: m.createdAt,
        isRead: m.isRead
      }));
      
      setMessages(formattedMessages);
      
      const unreadIds = messagesData
        .filter((m: any) => m.receiverId === user?.id && !m.isRead)
        .map((m: any) => m.id);
      
      if (unreadIds.length > 0) {
        await api.post('/messages/mark-read', { messageIds: unreadIds });
        fetchConversations();
      }
    } catch (error) {
      console.error('Mesajlar yüklenemedi:', error);
    }
  };

  const sendMessage = async () => {
    if (!newMessage.trim() || !selectedConversation) return;

    try {
      await api.post('/messages', {
        applicationId: selectedConversation,
        content: newMessage.trim(),
      });
      setNewMessage('');
      await fetchMessages(selectedConversation);
      await fetchConversations();
    } catch (error: any) {
      alert(error.response?.data?.message || 'Mesaj gönderilemedi. Lütfen tekrar deneyin.');
    }
  };

  return (
    <div className="min-h-screen pt-24 pb-8" style={{ backgroundColor: '#fffcf1' }}>
      <div className="container mx-auto px-4">
        <div className="max-w-6xl mx-auto">
          <h1 className="text-3xl font-bold mb-6">Mesajlar</h1>

          <div className="grid md:grid-cols-3 gap-6 h-[600px]">
            {/* Conversations List */}
            <Card className="overflow-y-auto">
              {isLoading ? (
                <div className="p-8 text-center">
                  <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
                </div>
              ) : conversations.length === 0 ? (
                <div className="p-8 text-center text-gray-600">
                  <MessageCircle className="w-12 h-12 mx-auto mb-4 text-gray-400" />
                  <p>Henüz mesajınız yok</p>
                </div>
              ) : (
                <div className="divide-y">
                  {conversations.map((conv) => (
                    <div
                      key={conv.applicationId}
                      className={`p-4 cursor-pointer hover:bg-gray-50 transition ${
                        selectedConversation === conv.applicationId ? 'bg-blue-50' : ''
                      }`}
                      onClick={() => setSelectedConversation(conv.applicationId)}
                    >
                      <div className="flex items-center justify-between mb-1">
                        <h3 className="font-semibold">{conv.otherUserName}</h3>
                        {conv.unreadCount > 0 && (
                          <span className="bg-blue-600 text-white text-xs px-2 py-1 rounded-full">
                            {conv.unreadCount}
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-gray-600 truncate">{conv.listingTitle}</p>
                      <p className="text-xs text-gray-500 mt-1 truncate">{conv.lastMessage}</p>
                    </div>
                  ))}
                </div>
              )}
            </Card>

            {/* Messages */}
            <div className="md:col-span-2">
              <Card className="h-full flex flex-col">
                {!selectedConversation ? (
                  <div className="flex-1 flex items-center justify-center text-gray-600">
                    <div className="text-center">
                      <MessageCircle className="w-16 h-16 mx-auto mb-4 text-gray-400" />
                      <p>Bir konuşma seçin</p>
                    </div>
                  </div>
                ) : (
                  <>
                    {/* Messages List */}
                    <div className="flex-1 overflow-y-auto p-4 space-y-4">
                      {messages.map((message) => {
                        const isMyMessage = message.senderId === user?.id;
                        return (
                          <div
                            key={message.id}
                            className={`flex ${isMyMessage ? 'justify-end' : 'justify-start'}`}
                          >
                            <div
                              className={`max-w-[70%] rounded-lg p-3 ${
                                isMyMessage
                                  ? 'bg-blue-600 text-white'
                                  : 'bg-gray-200 text-gray-900'
                              }`}
                            >
                              {!isMyMessage && (
                                <p className="text-xs font-medium mb-1 opacity-80">{message.senderName}</p>
                              )}
                              <p>{message.content}</p>
                              <p className={`text-xs mt-1 ${isMyMessage ? 'opacity-70' : 'text-gray-600'}`}>
                                {new Date(message.sentAt).toLocaleString('tr-TR', {
                                  day: 'numeric',
                                  month: 'short',
                                  hour: '2-digit',
                                  minute: '2-digit'
                                })}
                              </p>
                            </div>
                          </div>
                        );
                      })}
                    </div>

                    {/* Message Input */}
                    <div className="border-t p-4">
                      <div className="flex gap-2">
                        <Input
                          value={newMessage}
                          onChange={(e) => setNewMessage(e.target.value)}
                          placeholder="Mesajınızı yazın..."
                          onKeyPress={(e) => e.key === 'Enter' && sendMessage()}
                        />
                        <Button onClick={sendMessage}>
                          <Send className="w-5 h-5" />
                        </Button>
                      </div>
                    </div>
                  </>
                )}
              </Card>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}


