import type { Metadata } from "next"
import { Inter } from "next/font/google"
import "@/styles/globals.css"
import { AuthProvider } from "@/contexts/AuthContext"
import { Header } from "@/components/layout/Header"

const inter = Inter({ subsets: ["latin"] })

export const metadata: Metadata = {
  title: "Fitz Bot",
  description: "Fitz Bot Frontend",
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <AuthProvider>
          <Header />
          {children}
        </AuthProvider>
      </body>
    </html>
  )
}
