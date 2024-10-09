
export interface RegisterUser {
  User: {
    FirstName: string;
    LastName: string;
    Email: string;
    DateOfBirth: Date;
    Location: string;
    Price: number;
  };
  Password: string;
  ProfilePicture: File | null;
}
