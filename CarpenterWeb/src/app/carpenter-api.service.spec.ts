import { TestBed } from '@angular/core/testing';

import { CarpenterApiService } from './carpenter-api.service';

describe('CarpenterApiService', () => {
  let service: CarpenterApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CarpenterApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
