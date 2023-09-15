import { Injectable } from '@angular/core';
import { ChatMessage } from './models/chat-message';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { MessageGeneration } from './models/message-generation';

@Injectable({
  providedIn: 'root'
})
export class CarpenterApiService {

  constructor(private httpClient:HttpClient) { }

  async getChatMessages(): Promise<ChatMessage[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatMessages";

    return await lastValueFrom<ChatMessage[]>(this.httpClient.get<ChatMessage[]>(url));
  }

  async sendUserChatMessage(userChatMessage: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SendUserChatMessage";
    var body = userChatMessage;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async generateAIChatMessage():Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GenerateAIChatMessage";

    return await lastValueFrom(this.httpClient.get(url));
  }

  async getNotDoneMessageGenerations():Promise<MessageGeneration[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetNotDoneMessageGenerations";

    return await lastValueFrom<MessageGeneration[]>(this.httpClient.get<MessageGeneration[]>(url));
  }
}
