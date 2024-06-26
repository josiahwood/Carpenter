export interface MessageGeneration {
  // Identifiers
  id: string;
  userId: string;

  // Inputs
  model: string;
  worker: string;
  maxInputLength: number;
  maxOutputLength: number;
  prompt: string;
  purpose: string;
  nextPurpose: string;

  // Outputs
  stableHordeId: string;
  status: string;
  generatedOutput: string;
}
