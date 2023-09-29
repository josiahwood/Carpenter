export interface ChatMessage {
  id: string;
  userId: string;
  timestamp: Date;
  sender: string;
  message: string;
  messageGenerationId: string;
}
