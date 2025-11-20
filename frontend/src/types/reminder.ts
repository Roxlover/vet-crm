export interface ReminderDto {
  id: number;
  ownerName: string;
  ownerPhone: string;
  petName: string;
  dueDate: string; // DateOnly backend'den string gelecek
  procedureName?: string | null;
  isCompleted: boolean;
  visitImageUrl?: string | null;
}

export interface ReminderDashboardResponse {
  today: ReminderDto[];
  tomorrow: ReminderDto[];
  overdue: ReminderDto[];
  done: ReminderDto[];
}
