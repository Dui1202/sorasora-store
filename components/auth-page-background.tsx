import React, { PropsWithChildren } from "react";

const AuthPageBackground = ({ children }: PropsWithChildren) => {
  return (
    <div className="min-h-screen relative overflow-hidden bg-[linear-gradient(135deg,var(--primary),var(--secondary))]">
      {/* Glow layers */}
      <div
        className="absolute inset-0 
    bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.5),transparent_40%)]"
      />
      <div
        className="absolute inset-0 
    bg-[radial-gradient(circle_at_80%_60%,rgba(255,255,255,0.3),transparent_50%)]"
      />

      {/* Floating elements */}
      <div className="absolute left-10 bottom-20 w-48 h-64 bg-white/40 rounded-2xl shadow-xl rotate-[-8deg]" />
      <div className="absolute right-16 top-32 w-40 h-60 bg-white/40 rounded-2xl shadow-xl rotate-6" />
      {/* Form */}
      <div className="relative z-10 flex items-center justify-center min-h-screen">
        {children}
      </div>
    </div>
  );
};

export default AuthPageBackground;
