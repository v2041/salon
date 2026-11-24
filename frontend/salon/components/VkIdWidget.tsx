"use client";

import {Box} from "@mui/material";
import * as VKID from "@vkid/sdk";
import {useEffect, useRef} from "react";

interface VkIdWidgetProps {
    onSuccess: (payload: { code: string; deviceId: string; codeVerifier: string; redirectUrl: string }) => void;
    onError: (message: string) => void;
}

function generateCodeVerifier(): string {
    const array = new Uint8Array(32);
    crypto.getRandomValues(array);
    return btoa(String.fromCharCode(...array))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "");
}

export function VkIdWidget({onSuccess, onError}: VkIdWidgetProps) {
    const containerRef = useRef<HTMLDivElement>(null);
    // Храним актуальные колбэки в ref, чтобы не переинициализировать виджет при ререндерах
    const callbacksRef = useRef({onSuccess, onError});
    useEffect(() => {
        callbacksRef.current = {onSuccess, onError};
    });

    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;

        const appId = Number(process.env.NEXT_PUBLIC_VK_APP_ID);
        if (!appId) {
            console.error("Не задан NEXT_PUBLIC_VK_APP_ID");
            callbacksRef.current.onError("Вход через VK ID временно недоступен");
            return;
        }

        const codeVerifier = generateCodeVerifier();
        // Используем текущий origin, чтобы работало и на localhost, и на evgeniya-beauty.ru
        // (оба домена должны быть добавлены в кабинете VK ID)
        const redirectUrl = window.location.origin;

        VKID.Config.init({
            app: appId,
            redirectUrl,
            responseMode: VKID.ConfigResponseMode.Callback,
            source: VKID.ConfigSource.LOWCODE,
            codeVerifier,
            scope: "phone", // Запрашиваем телефон для сопоставления с клиентом в БД
        });

        const oneTap = new VKID.OneTap();

        oneTap
            .render({
                container,
                showAlternativeLogin: true,
                oauthList: [VKID.OAuthName.OK, VKID.OAuthName.MAIL],
            })
            .on(VKID.WidgetEvents.ERROR, () => {
                callbacksRef.current.onError("Ошибка авторизации через VK ID");
            })
            .on(VKID.OneTapInternalEvents.LOGIN_SUCCESS, (payload: { code: string; device_id: string }) => {
                callbacksRef.current.onSuccess({
                    code: payload.code,
                    deviceId: payload.device_id,
                    codeVerifier,
                    redirectUrl,
                });
            });

        return () => {
            oneTap.close();
            container.innerHTML = "";
        };
    }, []);

    return <Box ref={containerRef} sx={{width: "100%", minHeight: 44}} />;
}
