import { Injectable } from '@angular/core';
import { ChatMessage } from './models/chat-message';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { MessageGeneration } from './models/message-generation';
import { QuickMessage } from './models/quick-message';

@Injectable({
  providedIn: 'root'
})
export class CarpenterApiService {

  constructor(private httpClient: HttpClient) { }

  async getChatMemory(): Promise<string> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatMemory";
    return await lastValueFrom<string>(this.httpClient.get(url, { responseType: 'text' }));
  }

  async setChatMemory(chatMemory: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SetChatMemory";
    var body = chatMemory;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async getChatSummarizationPrompt(): Promise<string> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatSummarizationPrompt";
    return await lastValueFrom<string>(this.httpClient.get(url, { responseType: 'text' }));
  }

  async setChatSummarizationPrompt(chatSummarizationPrompt: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SetChatSummarizationPrompt";
    var body = chatSummarizationPrompt;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async getChatMessages(): Promise<ChatMessage[]> {
    //return [
    //  {
    //    id: "1234",
    //    userId: "0000",
    //    timestamp: new Date(),
    //    sender: "User",
    //    message: "This is the message that is really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really really long."
    //  }
    //];

    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatMessages";

    return await lastValueFrom<ChatMessage[]>(this.httpClient.get<ChatMessage[]>(url));
  }

  async sendUserChatMessage(userChatMessage: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SendUserChatMessage";
    var body = userChatMessage;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async editChatMessage(chatMessage: ChatMessage): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/EditChatMessage";
    var body = JSON.stringify(chatMessage);

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async deleteChatMessage(id: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/DeleteChatMessage?id=" + id;

    return await lastValueFrom(this.httpClient.get(url));
  }

  async getQuickMessages(): Promise<QuickMessage[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetQuickMessages";

    return await lastValueFrom<QuickMessage[]>(this.httpClient.get<QuickMessage[]>(url));
  }

  async createQuickMessage(quickMessage: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/CreateQuickMessage";
    var body = quickMessage;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async editQuickMessage(quickMessage: QuickMessage): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/EditQuickMessage";
    var body = JSON.stringify(quickMessage);

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async generateAIChatMessage():Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GenerateAIChatMessage";

    return await lastValueFrom(this.httpClient.get(url));
  }

  async getMessageGenerations(): Promise<MessageGeneration[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetMessageGenerations";

    return await lastValueFrom<MessageGeneration[]>(this.httpClient.get<MessageGeneration[]>(url));
  }

  async getNotDoneMessageGenerations(): Promise<MessageGeneration[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetNotDoneMessageGenerations";

    return await lastValueFrom<MessageGeneration[]>(this.httpClient.get<MessageGeneration[]>(url));
  }

  async importChatLog(chatLog: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/ImportChatLog";
    var body = chatLog;

    return await lastValueFrom(this.httpClient.post(url, body));
  }
}
