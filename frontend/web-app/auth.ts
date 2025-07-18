import NextAuth, { Profile } from "next-auth";
import { OIDCConfig } from "next-auth/providers";
import DuendeIDS6Provider from "next-auth/providers/duende-identity-server6";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    DuendeIDS6Provider({
      id: "id-server",
      clientId: "interactive.confidential",
      clientSecret: "secret",
      issuer: "http://localhost:5000",
      authorization: { params: { scope: "openid profile auctionApp" } },
      idToken: true,
    } as OIDCConfig<Profile>),
  ],
});
