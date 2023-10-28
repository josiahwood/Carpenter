import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatContextsComponent } from './chat-contexts.component';

describe('ChatContextsComponent', () => {
  let component: ChatContextsComponent;
  let fixture: ComponentFixture<ChatContextsComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatContextsComponent]
    });
    fixture = TestBed.createComponent(ChatContextsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
