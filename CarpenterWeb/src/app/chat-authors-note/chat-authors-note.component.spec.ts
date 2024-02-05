import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatAuthorsNoteComponent } from './chat-authors-note.component';

describe('ChatAuthorsNoteComponent', () => {
  let component: ChatAuthorsNoteComponent;
  let fixture: ComponentFixture<ChatAuthorsNoteComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ChatAuthorsNoteComponent]
    });
    fixture = TestBed.createComponent(ChatAuthorsNoteComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
