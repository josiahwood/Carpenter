import { Injectable } from '@angular/core';
import { ChatMessage } from './models/chat-message';
import { HttpClient, HttpParams } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { MessageGeneration } from './models/message-generation';
import { QuickMessage } from './models/quick-message';
import { ChatSummaryLog } from './models/chat-summary-log';
import { ChatContext } from './models/chat-context';

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

  async getChatAuthorsNote(): Promise<string> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatAuthorsNote";
    return await lastValueFrom<string>(this.httpClient.get(url, { responseType: 'text' }));
  }

  async setChatAuthorsNote(chatAuthorsNote: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SetChatAuthorsNote";
    var body = chatAuthorsNote;

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

  async sendUserChatMessage(userChatMessage: string): Promise<ChatMessage> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/SendUserChatMessage";
    var body = userChatMessage;

    return await lastValueFrom<ChatMessage>(this.httpClient.post<ChatMessage>(url, body));
  }

  async editChatMessage(chatMessage: ChatMessage): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/EditChatMessage";
    var body = JSON.stringify(chatMessage);

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async deleteChatMessage(id: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/DeleteChatMessage";
    var params = new HttpParams();
    params = params.set("id", id)

    return await lastValueFrom(this.httpClient.get(url, { params: params }));
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

  async generateUserChatMessage(): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GenerateUserChatMessage";

    return await lastValueFrom(this.httpClient.get(url));
  }

  async executeChatInstruction(instruction: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/ExecuteChatInstruction";
    var body = instruction;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async getMessageGenerations(): Promise<MessageGeneration[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetMessageGenerations";

    return await lastValueFrom<MessageGeneration[]>(this.httpClient.get<MessageGeneration[]>(url));
  }

  async getNotDoneMessageGenerations(): Promise<MessageGeneration[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetNotDoneMessageGenerations";

    return await lastValueFrom<MessageGeneration[]>(this.httpClient.get<MessageGeneration[]>(url));
  }

  async getMessageGeneration(id: string): Promise<MessageGeneration> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetMessageGeneration";
    var params = new HttpParams();
    params = params.set("id", id)

    return await lastValueFrom<MessageGeneration>(this.httpClient.get<MessageGeneration>(url, { params: params }));
  }

  async getLatestChatInstructionResponse(): Promise<string> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetLatestChatInstructionResponse";

    return await lastValueFrom<string>(this.httpClient.get(url, { responseType: 'text' }));
  }

  async importChatLog(chatLog: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/ImportChatLog";
    var body = chatLog;

    return await lastValueFrom(this.httpClient.post(url, body));
  }

  async compareModels(winnerModel: string, loserModel: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/CompareModels";
    var params = new HttpParams();
    params = params.set("winnerModel", winnerModel);
    params = params.set("loserModel", loserModel);

    return await lastValueFrom(this.httpClient.get(url, { params: params }));
  }

  async getChatSummaryLogs(): Promise<ChatSummaryLog[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/GetChatSummaryLogs";

    return await lastValueFrom<ChatSummaryLog[]>(this.httpClient.get<ChatSummaryLog[]>(url));
  }

  async deleteChatSummary(id: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/DeleteChatSummary";
    var params = new HttpParams();
    params = params.set("id", id)

    return await lastValueFrom(this.httpClient.get(url, { params: params }));
  }

  // chat-contexts

  async getChatContext(id: string): Promise<ChatContext> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/chat-contexts/" + id; 
    return await lastValueFrom<ChatContext>(this.httpClient.get<ChatContext>(url));
  }

  async getChatContexts(): Promise<ChatContext[]> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/chat-contexts";
    return await lastValueFrom<ChatContext[]>(this.httpClient.get<ChatContext[]>(url));
  }

  async createChatContext(chatContext: ChatContext): Promise<ChatContext> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/chat-contexts";
    var body = JSON.stringify(chatContext);
    return await lastValueFrom<ChatContext>(this.httpClient.post<ChatContext>(url, body));
  }

  async updateChatContext(chatContext: ChatContext): Promise<ChatContext> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/chat-contexts";
    var body = JSON.stringify(chatContext);
    return await lastValueFrom<ChatContext>(this.httpClient.put<ChatContext>(url, body));
  }

  async deleteCarpenterUser(): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/carpenter-users";
    return await lastValueFrom(this.httpClient.delete(url));
  }

  async deleteChatContext(id: string): Promise<Object> {
    var url = "https://zealous-wave-0e26a4710.3.azurestaticapps.net/api/chat-contexts/" + id;
    return await lastValueFrom(this.httpClient.delete(url));
  }
}
