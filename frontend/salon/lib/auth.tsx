"use client";

import {createContext, useCallback, useContext, useEffect, useState} from "react";
import type {ReactNode} from "react";
import {authApi} from "./api";
import type {CurrentUser} from "./types";

type AuthValue = {
    user: CurrentUser | null;
    loading: boolean;
    refresh: () => Promise<void>;
    signOut: () => Promise<void>;
};

const AuthContext = createContext<AuthValue | null>(null);

export function AuthProvider({children}: {children: ReactNode}) {
    const [user, setUser] = useState<CurrentUser | null>(null);
    const [loading, setLoading] = useState(true);

    const refresh = useCallback(async () => {
        try {
            setUser(await authApi.me());
        } catch {
            setUser(null);
        }
    }, []);

    useEffect(() => {
        let cancelled = false;
        (async () => {
            try {
                const me = await authApi.me();
                if (!cancelled) setUser(me);
            } catch {
                if (!cancelled) setUser(null);
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();
        return () => {
            cancelled = true;
        };
    }, []);

    const signOut = useCallback(async () => {
        await authApi.logout();
        setUser(null);
    }, []);

    return (
        <AuthContext.Provider value={{user, loading, refresh, signOut}}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const value = useContext(AuthContext);
    if (!value) throw new Error("useAuth вне AuthProvider");
    return value;
}
