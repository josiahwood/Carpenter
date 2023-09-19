import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportChatLogComponent } from './import-chat-log.component';

describe('ImportChatLogComponent', () => {
  let component: ImportChatLogComponent;
  let fixture: ComponentFixture<ImportChatLogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [ImportChatLogComponent]
    });
    fixture = TestBed.createComponent(ImportChatLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
