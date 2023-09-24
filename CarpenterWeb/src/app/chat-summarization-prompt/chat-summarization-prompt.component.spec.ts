import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatSummarizationPromptComponent } from './chat-summarization-prompt.component';

describe('ChatSummarizationPromptComponent', () => {
  let component: ChatSummarizationPromptComponent;
  let fixture: ComponentFixture<ChatSummarizationPromptComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatSummarizationPromptComponent]
    });
    fixture = TestBed.createComponent(ChatSummarizationPromptComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
