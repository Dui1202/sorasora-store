import React from "react";

const Background = () => {
  return (
    <div
      className="min-h-screen relative overflow-hidden
  bg-[linear-gradient(135deg,#cfe7f2,#e6d4e8)]"
    >
      <div
        className="absolute inset-0 
    bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.5),transparent_40%)]"
      />
      <div
        className="absolute inset-0 
    bg-[radial-gradient(circle_at_80%_60%,rgba(255,255,255,0.3),transparent_50%)]"
      />

      <div className="absolute left-10 bottom-20 w-48 h-64 bg-white/40 rounded-2xl shadow-xl rotate-[-8deg]" />
      <div className="absolute right-16 top-32 w-40 h-60 bg-white/40 rounded-2xl shadow-xl rotate-[6deg]" />
    </div>
  );
};

export default Background;
