import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteUserDataComponent } from './delete-user-data.component';

describe('DeleteUserDataComponent', () => {
  let component: DeleteUserDataComponent;
  let fixture: ComponentFixture<DeleteUserDataComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [DeleteUserDataComponent]
    });
    fixture = TestBed.createComponent(DeleteUserDataComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
