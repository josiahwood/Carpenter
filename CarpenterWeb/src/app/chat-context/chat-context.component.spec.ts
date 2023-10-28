import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatContextComponent } from './chat-context.component';

describe('ChatContextComponent', () => {
  let component: ChatContextComponent;
  let fixture: ComponentFixture<ChatContextComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatContextComponent]
    });
    fixture = TestBed.createComponent(ChatContextComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
