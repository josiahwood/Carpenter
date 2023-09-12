import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatMemoryComponent } from './chat-memory.component';

describe('ChatMemoryComponent', () => {
  let component: ChatMemoryComponent;
  let fixture: ComponentFixture<ChatMemoryComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatMemoryComponent]
    });
    fixture = TestBed.createComponent(ChatMemoryComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
